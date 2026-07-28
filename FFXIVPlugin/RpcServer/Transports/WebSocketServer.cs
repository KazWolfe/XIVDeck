using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.ImGuiNotification;
using Dalamud.Plugin.Services;
using Serilog;
using StreamJsonRpc;
using XIVDeck.FFXIVPlugin.Config;
using XIVDeck.FFXIVPlugin.Resources.Localization;
using XIVDeck.FFXIVPlugin.UI;
using XIVDeck.FFXIVPlugin.UI.Windows;

namespace XIVDeck.FFXIVPlugin.RpcServer.Transports;

/// <summary>
/// Minimal websocket server implementation for XIVDeck. Used instead of bringing in heavier or more annoying systems
/// like Kestrel, WS.Net, or EmbedIO, as we only really need the bare minimum to link up StreamJsonRPC.
/// </summary>
public class WebSocketServer : IRpcTransport {
    public const int DefaultWebsocketPort = 37984;

    private const string WebSocketGuid = "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";
    private const int MaxHandshakeHeaderBytes = 16 * 1024;
    private static readonly TimeSpan HandshakeHeaderTimeout = TimeSpan.FromSeconds(10);

    private readonly ILogger _log;
    private readonly PluginConfig _config;
    private readonly UIManager _uiManager;
    private readonly INotificationManager _notifications;

    private IActiveNotification? _portInUseNotification;

    private TcpListener? _listenerV4;
    private TcpListener? _listenerV6;
    private CancellationTokenSource? _cts;
    private Task? _acceptLoopV4;
    private Task? _acceptLoopV6;

    public Config.TransportType Type => Config.TransportType.WebSocket;

    public bool IsRunning => this._acceptLoopV4 is {IsCompleted: false} || this._acceptLoopV6 is {IsCompleted: false};

    public event Action<IJsonRpcMessageHandler>? Connected;

    private WebSocketTransportConfig Options => (WebSocketTransportConfig) this._config.GetTransport(Config.TransportType.WebSocket);

    public WebSocketServer(ILogger logger, PluginConfig config, UIManager uiManager, INotificationManager notifications) {
        this._log = logger;
        this._config = config;
        this._uiManager = uiManager;
        this._notifications = notifications;
    }

    public void StartServer() {
        this._log.Information("Starting JSON-RPC WebSocket server on port {Port}", this.Options.Port);
        this._cts = new CancellationTokenSource();

        try {
            this._listenerV4 = this.TryStartListener(IPAddress.Loopback);
            this._listenerV6 = this.TryStartListener(IPAddress.IPv6Loopback);

            this._portInUseNotification?.DismissNow();
            this._portInUseNotification = null;
        } catch (SocketException ex) when (ex.SocketErrorCode == SocketError.AddressAlreadyInUse) {
            this._log.Warning(ex, "Port {Port} is already in use, failed to bind JSON-RPC WebSocket server", this.Options.Port);
            this.RaisePortInUseNotification();
            throw new HandledTransportStartException("The configured WebSocket port is already in use.", ex);
        }

        this._acceptLoopV4 = Task.Run(() => this.AcceptLoopAsync(this._listenerV4, this._cts.Token), this._cts.Token);
        this._acceptLoopV6 = Task.Run(() => this.AcceptLoopAsync(this._listenerV6, this._cts.Token), this._cts.Token);
    }

    private TcpListener TryStartListener(IPAddress address) {
        var listener = new TcpListener(address, this.Options.Port);
        listener.Start();
        return listener;
    }

    public void StopServer() {
        this._cts?.Cancel();
        this._listenerV4?.Stop();
        this._listenerV6?.Stop();

        var loops = new[] {this._acceptLoopV4, this._acceptLoopV6}.Where(t => t != null).Select(t => t!).ToArray();
        try {
            Task.WaitAll(loops);
        } catch (AggregateException) {
            // expected on cancellation
        }
    }

    public void Dispose() {
        this.StopServer();

        GC.SuppressFinalize(this);
    }

    private async Task AcceptLoopAsync(TcpListener listener, CancellationToken token) {
        while (!token.IsCancellationRequested) {
            TcpClient client;
            try {
                client = await listener.AcceptTcpClientAsync(token);
            } catch (Exception) when (token.IsCancellationRequested) {
                break;
            }

            _ = this.HandleClientAsync(client, token);
        }

        this._log.Debug("JSON-RPC WebSocket accept loop has been stopped.");
    }

    private async Task HandleClientAsync(TcpClient client, CancellationToken token) {
        try {
            var stream = client.GetStream();

            var requestHeaders = await ReadHttpHeadersAsync(stream, token);

            // Block connections from browsers to prevent CSRF to FFXIV.
            if (FindHeaderValue(requestHeaders, "Origin") != null) {
                this._log.Warning("Rejected a connection from a browser (?!)");
                await WriteAsciiAsync(stream, "HTTP/1.1 403 Forbidden\r\nConnection: close\r\n\r\n", token);
                client.Dispose();
                return;
            }

            var webSocketKey = FindHeaderValue(requestHeaders, "Sec-WebSocket-Key");

            if (webSocketKey == null) {
                await WriteAsciiAsync(stream, "HTTP/1.1 400 Bad Request\r\nConnection: close\r\n\r\n", token);
                client.Dispose();
                return;
            }

            var acceptKey = Convert.ToBase64String(SHA1.HashData(Encoding.ASCII.GetBytes(webSocketKey + WebSocketGuid)));
            await WriteAsciiAsync(
                stream,
                "HTTP/1.1 101 Switching Protocols\r\n" +
                "Upgrade: websocket\r\n" +
                "Connection: Upgrade\r\n" +
                $"Sec-WebSocket-Accept: {acceptKey}\r\n\r\n",
                token);

            var webSocket = WebSocket.CreateFromStream(stream, isServer: true, subProtocol: null, keepAliveInterval: TimeSpan.FromSeconds(30));
            this.Connected?.Invoke(new WebSocketMessageHandler(webSocket, RpcFormatter.Create()));
        } catch (Exception ex) when (!token.IsCancellationRequested) {
            this._log.Warning(ex, "Failed to accept incoming WebSocket connection.");
            client.Dispose();
        }
    }

    private static async Task<string> ReadHttpHeadersAsync(NetworkStream stream, CancellationToken token) {
        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(token);
        timeoutCts.CancelAfter(HandshakeHeaderTimeout);

        var buffer = new MemoryStream();
        var single = new byte[1];

        try {
            while (true) {
                var read = await stream.ReadAsync(single, timeoutCts.Token);
                if (read == 0) break;

                buffer.WriteByte(single[0]);

                if (buffer.Length > MaxHandshakeHeaderBytes) {
                    throw new InvalidOperationException("WebSocket handshake headers exceeded the maximum allowed size.");
                }

                var length = buffer.Length;
                if (length >= 4) {
                    var b = buffer.GetBuffer();
                    if (b[length - 4] == '\r' && b[length - 3] == '\n' && b[length - 2] == '\r' && b[length - 1] == '\n') {
                        break;
                    }
                }
            }
        } catch (OperationCanceledException) when (!token.IsCancellationRequested) {
            throw new TimeoutException("Timed out waiting for WebSocket handshake headers.");
        }

        return Encoding.ASCII.GetString(buffer.GetBuffer(), 0, (int) buffer.Length);
    }

    private static string? FindHeaderValue(string headers, string name) {
        foreach (var line in headers.Split("\r\n")) {
            var idx = line.IndexOf(':');
            if (idx < 0) continue;

            if (string.Equals(line[..idx].Trim(), name, StringComparison.OrdinalIgnoreCase)) {
                return line[(idx + 1)..].Trim();
            }
        }

        return null;
    }

    private static Task WriteAsciiAsync(NetworkStream stream, string text, CancellationToken token) =>
        stream.WriteAsync(Encoding.ASCII.GetBytes(text), token).AsTask();

    private void RaisePortInUseNotification() {
        if (this._portInUseNotification != null) return;

        this._portInUseNotification = this._notifications.AddNotification(new Notification {
            Title = UIStrings.WebSocketServer_PortInUse_Title,
            Content = $"{string.Format(UIStrings.WebSocketServer_PortInUse_PortAlreadyInUse, this.Options.Port)} " +
                      $"{UIStrings.WebSocketServer_PortInUse_ResolutionInstructions}",
            Type = NotificationType.Warning,
            InitialDuration = TimeSpan.MaxValue,
        });

        this._portInUseNotification.DrawActions += args => {
            if (ImGui.Button(UIStrings.Common_OpenSettingsButton)) {
                this._uiManager.ShowOrFocus<SettingsWindow>();
            }

            ImGui.SameLine();
            if (ImGui.Button(UIStrings.Common_IgnoreForNowButton)) {
                args.Notification.DismissNow();
            }
        };
    }
}

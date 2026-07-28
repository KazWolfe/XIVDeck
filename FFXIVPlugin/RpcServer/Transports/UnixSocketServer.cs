using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Dalamud.Utility;
using Serilog;
using XIVDeck.FFXIVPlugin.Config;
using StreamJsonRpc;
using XIVDeck.FFXIVPlugin.Utils;

namespace XIVDeck.FFXIVPlugin.RpcServer.Transports;

/// <summary>
/// UNIX domain socket transport for XIVDeck.
/// Note that the thread loop here is *not* async due to a Wine bug with overlapped I/O.
/// </summary>
public class UnixSocketServer : IRpcTransport {
    private const string SocketNameBase = "XIVDeck";
    private const int AcceptPollIntervalMicroseconds = 250_000;

    private readonly ILogger _log;

    private Socket? _listenSocket;
    private CancellationTokenSource? _cts;
    private Task? _acceptLoopTask;

    public TransportType Type => TransportType.UnixDomainSocket;

    public string SocketPath { get; }

    public bool IsRunning => this._acceptLoopTask is { IsCompleted: false };

    public event Action<IJsonRpcMessageHandler>? Connected;

    public UnixSocketServer(ILogger log) {
        this._log = log;
        this.SocketPath = Path.Combine(this.DetectTempPath(), $"{SocketNameBase}-{Environment.ProcessId}.sock");
    }

    public void StartServer() {
        this.PruneDeadSockets();

        var socket = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified);
        socket.Bind(new UnixDomainSocketEndPoint(this.SocketPath));
        socket.Listen();
        this._listenSocket = socket;

        this._cts = new CancellationTokenSource();

        this._acceptLoopTask = Task.Factory.StartNew(
            () => this.AcceptLoop(socket, this._cts.Token),
            this._cts.Token,
            TaskCreationOptions.LongRunning,
            TaskScheduler.Default);
    }

    public void StopServer() {
        this._cts?.Cancel();

        try {
            this._acceptLoopTask?.Wait();
        } catch (AggregateException) {
            // expected on cancellation
        }

        this._listenSocket?.Dispose();
        this._listenSocket = null;

        this.WineSafeDeleteSockets(this.SocketPath);
    }

    public void Dispose() {
        this.StopServer();

        GC.SuppressFinalize(this);
    }

    private void AcceptLoop(Socket listenSocket, CancellationToken token) {
        this._log.Information("Starting JSON-RPC Unix socket server on '{SocketPath}'", this.SocketPath);

        while (!token.IsCancellationRequested) {
            try {
                if (!listenSocket.Poll(AcceptPollIntervalMicroseconds, SelectMode.SelectRead)) {
                    continue;
                }

                var client = listenSocket.Accept();
                var stream = new NetworkStream(client, ownsSocket: true);
                this.Connected?.Invoke(new LengthHeaderMessageHandler(stream, stream, RpcFormatter.Create()));
            } catch (ObjectDisposedException) {
                break;
            } catch (Exception ex) {
                this._log.Error(ex, "JSON-RPC Unix socket accept loop failed unexpectedly.");
                break;
            }
        }

        this._log.Debug("JSON-RPC Unix socket server has been stopped.");
    }

    private string DetectTempPath() {
        if (Util.IsWine()) {
            var sharedTmp = WineUtil.GetSharedTempDir();

            if (sharedTmp == null) {
                this._log.Warning("Couldn't determine tmp path, falling back to default");
                return Path.GetTempPath();
            }

            return sharedTmp;
        }

        return Path.GetTempPath();
    }

    private void PruneDeadSockets() {
        var dir = Path.GetDirectoryName(this.SocketPath);
        if (dir == null) {
            return;
        }

        IEnumerable<string> candidates;
        try {
            candidates = Directory.EnumerateFiles(dir, $"{SocketNameBase}-*.sock");
        } catch (Exception ex) {
            this._log.Warning(ex, "Could not scan '{Dir}' for stale sockets.", dir);
            return;
        }

        foreach (var candidate in candidates) {
            if (this.IsSocketAlive(candidate)) {
                this._log.Debug("Live socket found at '{Path}'.", candidate);
                continue;
            }

            this._log.Debug("Pruning dead UDS socket file '{Path}'.", candidate);
            this.WineSafeDeleteSockets(candidate);
        }
    }

    private bool IsSocketAlive(string path) {
        try {
            using var attempt = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified);
            attempt.Connect(new UnixDomainSocketEndPoint(path));
            return true;
        } catch {
            return false;
        }
    }

    /// <summary>
    /// Deletes a path using a Wine-safe mechanism. Since Wine on XLCore cannot delete UNIX Domain Sockets on Z:\
    /// (for <i>some reason</i>), this method will attempt a normal delete and then fall back to running <c>/bin/rm</c>
    /// on any Wine system.
    ///
    /// Since my sanity is limited, this won't verify whether the <c>rm</c> actually worked. If it doesn't, the user's
    /// probably having a bad day for other reasons.
    ///
    /// TODO: Remove this once wine gets their crap together.
    /// </summary>
    /// <param name="path">The path to safely delete.</param>
    private void WineSafeDeleteSockets(string path) {
        try {
            File.Delete(path);
        } catch (IOException ex) when (Util.IsWine() && ex.HResult == unchecked((int) 0x80070006)) { // WINE BUG!
            this._log.Warning(ex,
                "Failed to delete UDS socket file {Path} via WinAPI, trying Unix rm", path);
            var unixPath = WineUtil.WineToUnixPath(path);

            if (unixPath == null) {
                this._log.Warning("UDS path '{Path}' couldn't be translated to a unix path.",
                    path);
                return;
            }

            if (!WineUtil.RunNativeCommand("/bin/rm", "-f", unixPath)) {
                this._log.Warning("UNIX RM failed to execute for path: '{Path}'.", path);
            }
        } catch (Exception ex) {
            this._log.Error(ex, "Exception when deleting socket file '{Path}'.", path);
        }
    }
}

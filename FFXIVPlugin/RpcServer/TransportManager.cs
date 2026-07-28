using System;
using System.Linq;
using System.Net.Sockets;
using Autofac.Features.Indexed;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.ImGuiNotification;
using Dalamud.Plugin.Services;
using Dalamud.Utility;
using Serilog;
using StreamJsonRpc;
using XIVDeck.FFXIVPlugin.Config;
using XIVDeck.FFXIVPlugin.IoC;
using XIVDeck.FFXIVPlugin.Resources.Localization;
using XIVDeck.FFXIVPlugin.UI;
using XIVDeck.FFXIVPlugin.UI.Windows;

namespace XIVDeck.FFXIVPlugin.RpcServer;

[Service(ServiceFlags.Singleton)]
public class TransportManager : IDisposable {
    private readonly ILogger _log;
    private readonly PluginConfig _config;
    private readonly ConfigService _configService;
    private readonly IIndex<TransportType, IRpcTransport> _transports;
    private readonly UIManager _uiManager;
    private readonly INotificationManager _notifications;

    private IRpcTransport? _activeServer;

    /// <summary>
    /// Differs from <see cref="PluginConfig.ActiveTransport"/> when a fallback kicks in.
    /// </summary>
    public IRpcTransport? RunningTransport => this._activeServer;

    /// <inheritdoc cref="RunningTransport"/>
    public TransportType? RunningTransportType => this._activeServer?.Type;

    public event Action<IJsonRpcMessageHandler>? Connected;

    public TransportManager(ILogger logger, PluginConfig config, ConfigService configService,
        IIndex<TransportType, IRpcTransport> transports, UIManager uiManager, INotificationManager notifications) {
        this._log = logger;
        this._config = config;
        this._configService = configService;
        this._transports = transports;
        this._uiManager = uiManager;
        this._notifications = notifications;

        this.StartConfiguredTransport();
    }

    public void RestartTransport() {
        if (this._activeServer != null) {
            this._activeServer.Connected -= this.OnConnected;
            this._activeServer.StopServer();

            this._activeServer = null;
        }

        this.StartConfiguredTransport();
    }

    private void StartConfiguredTransport() {
        var type = this._config.ActiveTransport;

        if (!TransportPlatform.AllowedTypes.Contains(type)) {
            this._log.Warning(
                "Configured transport '{Type}' isn't reachable on this host - falling back to '{Fallback}'.",
                type, TransportPlatform.NativeDefault);

            type = TransportPlatform.NativeDefault;
        }

        this.StartTransport(type);
    }

    private void StartTransport(TransportType type) {
        if (!this._transports.TryGetValue(type, out var server)) {
            throw new InvalidOperationException($"Unknown transport type '{type}'.");
        }

        server.Connected += this.OnConnected;

        try {
            server.StartServer();

            this._activeServer = server;
            this._log.Information("Started {Type} JSON-RPC transport.", type);
        } catch (SocketException ex) when (ex.SocketErrorCode == SocketError.AddressFamilyNotSupported && type == TransportType.UnixDomainSocket) {
            this._log.Warning(ex, "'{Type}' transport isn't supported on this system - falling back to WebSocket for this session.", type);

            server.Connected -= this.OnConnected;
            this.RaiseUdsUnsupportedNotification();

            this.StartTransport(TransportType.WebSocket);
        } catch (HandledTransportStartException ex) {
            this._log.Warning(ex, "'{Type}' transport failed to start.", type);

            server.Connected -= this.OnConnected;
            this._activeServer = null;
        } catch (Exception ex) {
            this._log.Warning(ex, "Failed to start '{Type}' transport.", type);

            server.Connected -= this.OnConnected;
            this._activeServer = null;
            this.RaiseGenericFailureNotification();
        }
    }

    private void RaiseUdsUnsupportedNotification() {
        var resolutionText = Util.IsWine()
            ? UIStrings.TransportManager_UdsUnsupported_ResolutionInstructions_Wine
            : UIStrings.TransportManager_UdsUnsupported_ResolutionInstructions_NonWine;

        var notification = this._notifications.AddNotification(new Notification {
            Title = UIStrings.TransportManager_UdsUnsupported_Title,
            Content = $"{UIStrings.TransportManager_UdsUnsupported_Description} {resolutionText}",
            Type = NotificationType.Info,
            InitialDuration = TimeSpan.FromSeconds(15),
        });

        notification.DrawActions += args => {
            if (ImGui.Button(UIStrings.TransportManager_UdsUnsupported_SwitchButton)) {
                this._config.ActiveTransport = TransportType.WebSocket;
                this._configService.Save();
                args.Notification.DismissNow();
            }

            ImGui.SameLine();
            if (ImGui.Button(UIStrings.Common_IgnoreForNowButton)) {
                args.Notification.DismissNow();
            }
        };
    }

    private void RaiseGenericFailureNotification() {
        var notification = this._notifications.AddNotification(new Notification {
            Title = UIStrings.TransportManager_TransportUnavailable_Title,
            Content = $"{UIStrings.TransportManager_TransportUnavailable_Description} {UIStrings.TransportManager_TransportUnavailable_ResolutionInstructions}",
            Type = NotificationType.Error,
            InitialDuration = TimeSpan.FromSeconds(15),
        });

        notification.DrawActions += args => {
            if (ImGui.Button(UIStrings.Common_OpenSettingsButton)) {
                this._uiManager.ShowOrFocus<SettingsWindow>();
            }

            ImGui.SameLine();
            if (ImGui.Button(UIStrings.TransportManager_RetryButton)) {
                args.Notification.DismissNow();
                this.RestartTransport();
            }

            ImGui.SameLine();
            if (ImGui.Button(UIStrings.Common_IgnoreForNowButton)) {
                args.Notification.DismissNow();
            }
        };
    }

    private void OnConnected(IJsonRpcMessageHandler handler) {
        this.Connected?.Invoke(handler);
    }

    public void Dispose() {
        if (this._activeServer != null) {
            this._activeServer.Connected -= this.OnConnected;
        }

        GC.SuppressFinalize(this);
    }
}

using System;
using System.Linq;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Components;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Windowing;
using Dalamud.Utility;
using XIVDeck.FFXIVPlugin.Config;
using XIVDeck.FFXIVPlugin.IoC;
using XIVDeck.FFXIVPlugin.Resources.Localization;
using XIVDeck.FFXIVPlugin.RpcServer;
using XIVDeck.FFXIVPlugin.RpcServer.Transports;
using XIVDeck.FFXIVPlugin.Utils;

namespace XIVDeck.FFXIVPlugin.UI.Windows;

[Service(ServiceFlags.Transient)]
public class SettingsWindow : XIVDeckWindow {
    public const string WindowKey = "###xivDeckSettingsWindow";

    private static readonly TransportType[] TransportTypes = TransportPlatform.AllowedTypes.ToArray();
    private static readonly string[] TransportTypeLabels = TransportTypes.Select(GetTransportLabel).ToArray();

    private static string GetTransportLabel(TransportType type) => type switch {
        TransportType.WebSocket => UIStrings.SettingsWindow_TransportType_WebSocket,
        TransportType.UnixDomainSocket => UIStrings.SettingsWindow_TransportType_UnixDomainSocket,
        TransportType.NamedPipe => UIStrings.SettingsWindow_TransportType_NamedPipe,
        _ => type.ToString(),
    };

    private readonly ConfigService _configService;
    private readonly TransportManager _transportManager;
    private readonly PluginConfig _pluginConfig;

    // settings
    private int _transportTypeIndex;
    private int _websocketPort;
    private bool _safeMode = true;

    private string? _activePipeName;
    private string? _activeSocketPath;
    private string? _activeSocketPathNative;

    public SettingsWindow(UIManager uiManager, ConfigService configService, TransportManager transportManager, PluginConfig pluginConfig) :
        base(uiManager, WindowKey, ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoCollapse, true) {
        this._configService = configService;
        this._transportManager = transportManager;
        this._pluginConfig = pluginConfig;

        this.SizeCondition = ImGuiCond.FirstUseEver;
        this.SizeConstraints = new WindowSizeConstraints {
            MinimumSize = new Vector2(350, 250),
            MaximumSize = new Vector2(450, 400)
        };
        this.Size = this.SizeConstraints.Value.MinimumSize;
        this.WindowName = UIStrings.SettingsWindow_Title + WindowKey;
    }

    public override void OnOpen() {
        this._transportTypeIndex = Array.IndexOf(TransportTypes, this._pluginConfig.ActiveTransport);
        if (this._transportTypeIndex < 0) this._transportTypeIndex = 0;

        this._websocketPort = ((WebSocketTransportConfig) this._pluginConfig.GetTransport(TransportType.WebSocket)).Port;

        this._safeMode = this._pluginConfig.SafeMode;

        this.RefreshTransportInfoCache();
    }

    private void RefreshTransportInfoCache() {
        this._activePipeName = null;
        this._activeSocketPath = null;
        this._activeSocketPathNative = null;

        switch (this._transportManager.RunningTransport) {
            case NamedPipeServer pipe:
                this._activePipeName = pipe.PipeName;
                break;
            case UnixSocketServer uds:
                this._activeSocketPath = uds.SocketPath;

                if (Util.IsWine()) {
                    this._activeSocketPathNative = WineUtil.WineToUnixPath(uds.SocketPath);
                }

                break;
        }
    }

    private bool IsSelectedTransportActive() =>
        TransportTypes[this._transportTypeIndex] == this._transportManager.RunningTransportType;

    private TransportType? FallbackTransportType {
        get {
            var running = this._transportManager.RunningTransportType;
            return running != null && running != this._pluginConfig.ActiveTransport ? running : null;
        }
    }

    public override void Draw() {
        var windowSize = ImGui.GetContentRegionAvail();

        var pbs = ImGuiHelpers.GetButtonSize("placeholder");

        ImGui.BeginChild("SettingsPane", windowSize with {Y = windowSize.Y - pbs.Y - 6});

        if (!this._safeMode) {
            ImGui.PushTextWrapPos();
            ImGui.PushStyleColor(ImGuiCol.Text, ImGuiColors.DalamudRed);

            ImGui.Text(UIStrings.SettingsWindow_SafeModeDisabledWarning);

            ImGui.PopStyleColor();
            ImGui.PopTextWrapPos();
            ImGui.Spacing();
        }

        if (this.FallbackTransportType is {} fallback) {
            ImGui.PushTextWrapPos();
            ImGui.TextColored(ImGuiColors.DalamudYellow,
                string.Format(UIStrings.SettingsWindow_TransportFallbackNotice, GetTransportLabel(fallback)));
            ImGui.PopTextWrapPos();
            ImGui.Spacing();
        }

        ImGui.PushItemWidth(120);
        ImGui.Combo(UIStrings.SettingsWindow_TransportType, ref this._transportTypeIndex, TransportTypeLabels, TransportTypeLabels.Length);
        ImGui.PopItemWidth();
        ImGuiComponents.HelpMarker(UIStrings.SettingsWindow_TransportType_Help);

        if (TransportTypes[this._transportTypeIndex] == TransportType.WebSocket) {
            ImGui.PushItemWidth(80);

            if (ImGui.InputInt(UIStrings.SettingsWindow_APIPort, ref this._websocketPort)) {
                if (this._websocketPort < 1024) this._websocketPort = 1024;
                if (this._websocketPort > 49151) this._websocketPort = 49151;
            }

            ImGui.PopItemWidth();
            ImGuiComponents.HelpMarker(string.Format(UIStrings.SettingsWindow_APIPort_Help, WebSocketServer.DefaultWebsocketPort, 1024, 49151));

            ImGui.TextWrapped(string.Format(UIStrings.SettingsWindow_ListenIP, "localhost"));
        } else if (TransportTypes[this._transportTypeIndex] == TransportType.NamedPipe) {
            if (this.IsSelectedTransportActive() && this._activePipeName != null) {
                ImGui.PushTextWrapPos();
                ImGui.TextWrapped(string.Format(UIStrings.SettingsWindow_PipeName, this._activePipeName));
                ImGui.PopTextWrapPos();
            }
        } else if (TransportTypes[this._transportTypeIndex] == TransportType.UnixDomainSocket) {
            if (this.IsSelectedTransportActive() && this._activeSocketPath != null) {
                ImGui.PushTextWrapPos();
                ImGui.TextWrapped(string.Format(UIStrings.SettingsWindow_SocketPath, this._activeSocketPath));

                if (this._activeSocketPathNative != null) {
                    ImGui.TextWrapped(string.Format(UIStrings.SettingsWindow_SocketPath_Native, this._activeSocketPathNative));
                }

                ImGui.PopTextWrapPos();
            }
        }

        ImGui.EndChild();

        /* FOOTER */
        ImGui.Separator();

        if (ImGui.Button(UIStrings.SettingsWindow_GitHubLink)) UiUtil.OpenXIVDeckGitHub();

        var applyText = UIStrings.SettingsWindow_ApplyButton;
        var applyButtonSize = ImGuiHelpers.GetButtonSize(applyText);

        ImGui.SameLine(windowSize.X - applyButtonSize.X - 5);
        if (ImGui.Button(applyText)) {
            this.SaveSettings();
        }
    }

    private void SaveSettings() {
        var selectedType = TransportTypes[this._transportTypeIndex];

        var existingWsPort = ((WebSocketTransportConfig) this._pluginConfig.GetTransport(TransportType.WebSocket)).Port;
        var portChanged = selectedType == TransportType.WebSocket && existingWsPort != this._websocketPort;

        var transportChanged = this._pluginConfig.ActiveTransport != selectedType || portChanged;

        if (transportChanged) {
            if (selectedType == TransportType.WebSocket) {
                ((WebSocketTransportConfig) this._pluginConfig.GetTransport(TransportType.WebSocket)).Port = this._websocketPort;
            }

            this._pluginConfig.ActiveTransport = selectedType;
        }

        this._configService.Save();

        if (transportChanged) {
            this._transportManager.RestartTransport();
            this.RefreshTransportInfoCache();
        }
    }
}

using System;
using System.Globalization;
using System.Numerics;
using System.Reflection;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Windowing;
using EmbedIO;
using ImGuiNET;
using XIVDeck.FFXIVPlugin.Resources.Localization;
using XIVDeck.FFXIVPlugin.Server;
using XIVDeck.FFXIVPlugin.Server.Helpers;
using XIVDeck.FFXIVPlugin.Server.Messages.Outbound;
using XIVDeck.FFXIVPlugin.UI.Windows.Nags;
using XIVDeck.FFXIVPlugin.Utils;

namespace XIVDeck.FFXIVPlugin.UI.Windows;

// ReSharper disable once UnusedType.Global - Used in #DEBUG builds from SettingsWindow
public class DebugWindow : Window {
    private const string WindowKey = "XIVDeck Debug Tools";

    private static DebugWindow? _instance;

    internal static DebugWindow GetOrCreate() {
        if (_instance == null) {
            _instance = new DebugWindow();
            XIVDeckPlugin.Instance.WindowSystem.AddWindow(_instance);
        }

        return _instance;
    }
    
    // secret configs
    private bool _listenOnAllInterfaces;
    private int _httpListenerMode;

    private DebugWindow() : base(WindowKey) {
        this.Size = new Vector2(300, 250);
        this.SizeCondition = ImGuiCond.FirstUseEver;
    }

    public override void PreDraw() {
        base.PreDraw();
        
        this._listenOnAllInterfaces = XIVDeckPlugin.Instance.Configuration.ListenOnAllInterfaces;
        this._httpListenerMode = (int) XIVDeckPlugin.Instance.Configuration.HttpListenerMode;
    }

    public override void Draw() {
        ImGui.Text("Hello, world! Enjoying your time behind the curtain?");
        
        ImGui.Text("---Diagnostic Commands ---");
        ImGui.Indent();

        if (ImGui.Button("Clear Icons")) {
            XIVDeckWSServer.Instance?.BroadcastMessage(new WSStateUpdateMessage("DEBUG_ClearIcons"));
        }

        ImGui.SameLine();
        if (ImGui.Button("Redraw All Icons")) {
            XIVDeckWSServer.Instance?.BroadcastMessage(new WSStateUpdateMessage("IconCache"));
        }

        if (ImGui.Button("Resend Init Command")) {
            var xivPluginVersion = Assembly.GetExecutingAssembly().GetName().Version!;
            var packet = new WSInitReplyMessage(xivPluginVersion.GetMajMinBuild(), AuthHelper.Instance.Secret);
            XIVDeckWSServer.Instance?.BroadcastMessage(packet);
        }
        
        ImGui.Unindent();
        ImGui.Spacing();
        
        ImGui.Text("--- Server Control ---");
        ImGui.Indent();

        if (ImGui.Checkbox(UIStrings.SettingsWindow_ListenOnNetwork, ref this._listenOnAllInterfaces)) {
            XIVDeckPlugin.Instance.Configuration.ListenOnAllInterfaces = this._listenOnAllInterfaces;
        }

        var listenerModes = Enum.GetNames<HttpListenerMode>();
        if (ImGui.Combo("HTTP Listener Mode", ref this._httpListenerMode, listenerModes, listenerModes.Length)) {
            XIVDeckPlugin.Instance.Configuration.HttpListenerMode = (HttpListenerMode) this._httpListenerMode;
        }
        
        ImGui.PushStyleColor(ImGuiCol.Text, ImGuiColors.DalamudRed);
        if (ImGui.Button("KILL SERVER")) {
            var field = XIVDeckPlugin.Instance.GetType().GetField("_xivDeckWebServer",
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            var ws = (XIVDeckWebServer) field?.GetValue(XIVDeckPlugin.Instance)!;
            ws.Dispose();
        }

        ImGui.PopStyleColor();
        ImGui.SameLine();
        if (ImGui.Button("(Re)Start Server")) {
            XIVDeckPlugin.Instance.InitializeWebServer();
        }
        
        ImGui.Unindent();
        ImGui.Spacing();
        ImGui.Text("--- Localization ---");
        ImGui.Indent();
        
        if (ImGui.Button("Pseudo-localize")) {
            UIStrings.Culture = new CultureInfo("qps-ploc");
        }

        ImGui.Unindent();
        ImGui.Spacing();
        ImGui.Text("--- Nag Windows ---");
        ImGui.Indent();
        
        if (ImGui.Button("Reset Nags")) {
            NagWindow.CloseAllNags();
        }

        if (ImGui.Button("Open Forced Update Nag")) ForcedUpdateNag.Show();
        ImGui.SameLine();
        if (ImGui.Button("Open Setup Nag")) SetupNag.Show();
        ImGui.SameLine();
        if (ImGui.Button("Open Testing Update Nag")) TestingUpdateNag.Show();
        
        ImGui.Unindent();
    }
}
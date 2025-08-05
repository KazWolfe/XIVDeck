using System;
using System.Collections.Generic;
using System.Globalization;
using System.Numerics;
using System.Reflection;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Windowing;
using EmbedIO;
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
    private int _httpListenerMode;

    private DebugWindow() : base(WindowKey) {
        this.Size = new Vector2(300, 250);
        this.SizeCondition = ImGuiCond.FirstUseEver;
    }

    public override void PreDraw() {
        base.PreDraw();

        var listenerMode = XIVDeckPlugin.Instance.Configuration.HttpListenerMode;
        this._httpListenerMode = listenerMode != null ? (int) listenerMode.Value + 1 : 0;
    }

    public override void Draw() {
        ImGui.Text("Hello, world! Enjoying your time behind the curtain?");

        ImGui.Text("---Diagnostic Commands ---");
        ImGui.Indent();

        if (ImGui.Button("Clear Icons")) {
            XIVDeckPlugin.Instance.Server.BroadcastMessage(new WSStateUpdateMessage("DEBUG_ClearIcons"));
        }

        ImGui.SameLine();
        if (ImGui.Button("Redraw All Icons")) {
            XIVDeckPlugin.Instance.Server.BroadcastMessage(new WSStateUpdateMessage("IconCache"));
        }

        if (ImGui.Button("Resend Init Command")) {
            var xivPluginVersion = Assembly.GetExecutingAssembly().GetName().Version!;
            var packet = new WSInitReplyMessage(xivPluginVersion.GetMajMinBuild(), AuthHelper.Instance.Secret);
            XIVDeckPlugin.Instance.Server.BroadcastMessage(packet);
        }

        ImGui.Unindent();
        ImGui.Spacing();

        ImGui.Text("--- Server Control ---");
        ImGui.Indent();

        var listenerModes = new List<string> {"Default"};
        listenerModes.AddRange(Enum.GetNames<HttpListenerMode>());
        if (ImGui.Combo("HTTP Listener Mode", ref this._httpListenerMode, listenerModes.ToArray(), listenerModes.Count)) {
            XIVDeckPlugin.Instance.Configuration.HttpListenerMode = this._httpListenerMode == 0 ? null : (HttpListenerMode) this._httpListenerMode - 1;
            XIVDeckPlugin.Instance.Configuration.Save();
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

﻿using System.Numerics;
using Dalamud.Interface;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Components;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using XIVDeck.FFXIVPlugin.UI.Windows.Nags;

namespace XIVDeck.FFXIVPlugin.UI.Windows;

public class SettingsWindow : Window {
    public const string WindowKey = "XIVDeck Game Plugin Settings";

    private readonly XIVDeckPlugin _plugin = XIVDeckPlugin.Instance;

    // settings
    private int _websocketPort;
    private bool _safeMode = true;

    public SettingsWindow(bool forceMainWindow = true) :
        base(WindowKey, ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse, forceMainWindow) {
        
        this.Size = new Vector2(300, 150);
        this.SizeCondition = ImGuiCond.FirstUseEver;
        
        this.IsOpen = true;
    }

    public override void OnOpen() {
        this._websocketPort = this._plugin.Configuration.WebSocketPort;
        this._safeMode = this._plugin.Configuration.SafeMode;
    }

    public override void OnClose() {
        this._plugin.WindowSystem.RemoveWindow(this);
    }

    public override void Draw() {
        var windowSize = ImGui.GetWindowContentRegionMax();

        if (!this._safeMode) {
            ImGui.PushTextWrapPos();
            ImGui.PushStyleColor(ImGuiCol.Text, ImGuiColors.DalamudRed);

            ImGui.Text("DANGER: SAFE MODE DISABLED! You may be able to send illegal commands from your " +
                       "Stream Deck to the game.");

            ImGui.PopStyleColor();
            ImGui.PopTextWrapPos();
            ImGui.Spacing();
        }

        ImGui.PushItemWidth(80);

        if (ImGui.InputInt("API Port", ref this._websocketPort, 0)) {
            if (this._websocketPort < 1024) this._websocketPort = 1024;
            if (this._websocketPort > 59999) this._websocketPort = 59999;
        }

        ImGui.PopItemWidth();
        ImGuiComponents.HelpMarker("Default port: 37984\n\nRange: 1024-59999");

        ImGui.TextWrapped($"Listen IP: 127.0.0.1");

        /* FOOTER */
        var placeholderButtonSize = ImGuiHelpers.GetButtonSize("placeholder");

        ImGui.SetCursorPosY(windowSize.Y - placeholderButtonSize.Y - 2);
        ImGui.Separator();

        if (ImGui.Button("XIVDeck GitHub")) PluginUI.OpenXIVDeckGitHub();

        var applyText = "Apply Settings";
        var applyButtonSize = ImGuiHelpers.GetButtonSize(applyText);

        ImGui.SameLine(windowSize.X - applyButtonSize.X - 20);
        if (ImGui.Button(applyText)) {
            this.SaveSettings();
        }
    }

    private void SaveSettings() {
        if (this._websocketPort != this._plugin.Configuration.WebSocketPort) {
            this._plugin.Configuration.WebSocketPort = this._websocketPort;
            this._plugin.Configuration.HasLinkedStreamDeckPlugin = false;

            this._plugin.Configuration.Save();
            NagWindow.CloseAllNags();
            SetupNag.Show();
        }

        this._plugin.InitializeWebServer();
    }
}
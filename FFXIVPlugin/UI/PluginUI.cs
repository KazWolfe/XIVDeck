using System;
using System.Diagnostics;
using Dalamud.Interface;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Components;
using ImGuiNET;

namespace FFXIVPlugin.UI {
    class PluginUI : IDisposable {
        private XIVDeckPlugin _plugin;
    
        private bool _settingsVisible;
        public bool SettingsVisible {
            get { return this._settingsVisible; }
            set { this._settingsVisible = value; }
        }
        
        // imgui implementation
        private int _wsPort;

        // passing in the image here just for simplicity
        public PluginUI(XIVDeckPlugin plugin) {
            this._plugin = plugin;

            this._wsPort = this._plugin.Configuration.WebSocketPort;
        }

        public void Dispose() {
        }

        public void Draw() {
            // This is our only draw handler attached to UIBuilder, so it needs to be
            // able to draw any windows we might have open.
            // Each method checks its own visibility/state to ensure it only draws when
            // it actually makes sense.
            // There are other ways to do this, but it is generally best to keep the number of
            // draw delegates as low as possible.

            DrawSettingsWindow();
        }

        public void DrawSettingsWindow() {
            if (!this.SettingsVisible) {
                return;
            }
            
            if (ImGui.Begin("XIVDeck Configuration Menu", ref this._settingsVisible)) {
                var hasLinkedStreamDeck = this._plugin.Configuration.HasLinkedStreamDeckPlugin;
                var safeMode = this._plugin.Configuration.SafeMode;

                if (!safeMode) {
                    ImGui.PushTextWrapPos();
                    ImGui.PushStyleColor(ImGuiCol.Text, ImGuiColors.DalamudRed);
                    
                    ImGui.Text( "DANGER: SAFE MODE DISABLED! You may be able to send illegal commands from your " +
                                "Stream Deck to the game.");
                    
                    ImGui.PopStyleColor();
                    ImGui.PopTextWrapPos();
                    ImGui.Spacing();
                }

                if (!hasLinkedStreamDeck) {
                    ImGui.PushTextWrapPos();
                    ImGui.PushStyleColor(ImGuiCol.Text, ImGuiColors.DalamudYellow);

                    ImGui.Text("A Stream Deck has never been connected to this plugin instance. If you haven't " +
                               "done so already, please make sure you've downloaded and installed the Stream Deck " +
                               "plugin from XIVDeck's GitHub repo.");

                    if (ImGui.Button("Open XIVDeck GitHub")) this.OpenXIVDeckGitHub();
                    
                    ImGui.Text("You will also need to have configured at least " +
                               "one button on your Stream Deck and properly set connection information.");
                    
                    ImGui.PopStyleColor();
                    ImGui.PopTextWrapPos();
                    ImGui.Spacing();
                }
                
                ImGui.PushItemWidth(80);

                if (ImGui.InputInt("WebSocket Port", ref this._wsPort, 0)) {
                    if (this._wsPort < 1024) this._wsPort = 1024;
                    if (this._wsPort > 59999) this._wsPort = 59999;
                }
                
                ImGui.PopItemWidth();
                
                ImGuiComponents.HelpMarker("Default port: 37984\n\nRange: 1024-59999");
                ImGui.TextWrapped($"Listen IP: 127.0.0.1");

                // FOOTER //

                var windowSize = ImGui.GetWindowContentRegionMax();
                var placeholderButtonSize = ImGuiHelpers.GetButtonSize("placeholder");
                
                ImGui.SetCursorPosY(windowSize.Y - placeholderButtonSize.Y - 4);
                ImGui.Separator();
                
                if (ImGui.Button("XIVDeck GitHub")) this.OpenXIVDeckGitHub();

                var applyText = "Apply Settings";
                var applyButtonSize = ImGuiHelpers.GetButtonSize(applyText);

                ImGui.SameLine(windowSize.X - applyButtonSize.X - 20);
                if (ImGui.Button(applyText)) {
                    if (this._wsPort != this._plugin.Configuration.WebSocketPort) {
                        this._plugin.Configuration.WebSocketPort = this._wsPort;
                        this._plugin.Configuration.HasLinkedStreamDeckPlugin = false;
                        
                        this._plugin.Configuration.Save();
                    }
                    
                    this._plugin.InitializeWSServer();
                }
            }
            ImGui.End();
        }

        private void OpenXIVDeckGitHub() {
            Process.Start(new ProcessStartInfo() {
                FileName = Constants.GithubUrl,
                UseShellExecute = true,
            });
        }
    }
}
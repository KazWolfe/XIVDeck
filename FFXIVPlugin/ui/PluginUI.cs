using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Numerics;
using Dalamud;
using Dalamud.Data;
using Dalamud.Game.Command;
using Dalamud.Interface.Colors;
using Dalamud.Logging;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using FFXIVPlugin.helpers;
using FFXIVPlugin.Utils;
using Lumina.Excel.GeneratedSheets;
using Action = Lumina.Excel.GeneratedSheets.Action;

namespace FFXIVPlugin.ui {
    unsafe class PluginUI : IDisposable {
        private XIVDeckPlugin _plugin;
        
        string commandToExecute = string.Empty;

        private int query_hotbar_id = 0;
        private int query_item_id = 0;

        private int selected_action_type = 0;
        private int selected_action_id = 0;

        // this extra bool exists for ImGui, since you can't ref a property
        private bool visible = false;
        public bool Visible {
            get { return this.visible; }
            set { this.visible = value; }
        }

        private bool settingsVisible = false;
        public bool SettingsVisible {
            get { return this.settingsVisible; }
            set { this.settingsVisible = value; }
        }

        // passing in the image here just for simplicity
        public PluginUI(XIVDeckPlugin plugin) {
            this._plugin = plugin;
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

            DrawDebugWindow();
            DrawSettingsWindow();
        }

        public void DrawDebugWindow() {
            if (!Visible) {
                return;
            }

            ImGui.SetNextWindowSize(new Vector2(375, 330), ImGuiCond.FirstUseEver);
            ImGui.SetNextWindowSizeConstraints(new Vector2(375, 330), new Vector2(float.MaxValue, float.MaxValue));
            if (ImGui.Begin("XIVDeck Debug Window", ref this.visible)) {
                if (ImGui.Button("Show Settings")) {
                    SettingsVisible = true;
                }
                
                ImGui.InputText("Command", ref commandToExecute, 255);

                if (ImGui.Button("Run Command")) {
                    PluginLog.Debug($"Command: {commandToExecute}");
                    this._plugin.XivCommon.Functions.Chat.SendMessage(commandToExecute);
                }
                
                ImGui.Spacing();

                var hotbarModule =
                    FFXIVClientStructs.FFXIV.Client.System.Framework.Framework.Instance()->GetUiModule()->
                        GetRaptureHotbarModule();
                
                var hotbarItem = hotbarModule->HotBar[query_hotbar_id]->Slot[query_item_id];
                var icon = Injections.DataManager.GetImGuiTextureIcon(hotbarItem->Icon >= 1000000,
                    (uint) hotbarItem->Icon % 1000000);

                ImGui.Text("-- HOTBAR DEBUG --");
                ImGui.BeginGroup();
                ImGui.InputInt("hotbar number", ref query_hotbar_id);
                ImGui.InputInt("slot number", ref query_item_id);
                ImGui.EndGroup();
                ImGui.Text($"Information for action at bar {query_hotbar_id}, slot {query_item_id}:");
                ImGui.Indent();
                ImGui.Text($"Action Type: {hotbarItem -> CommandType}");
                ImGui.Text($"Raw ID: {hotbarItem -> CommandId}");
                // ImGui.Text($"Action Name: {action.Name}");
                if (icon != null) {
                    ImGui.Image(icon.ImGuiHandle, new Vector2(icon.Width, icon.Height));
                }

                if (ImGui.Button("TRIGGER")) {
                    _plugin.SigHelper.ExecuteHotbarSlot(hotbarItem);
                }
                
                ImGui.Unindent();
                
                ImGui.Text("-- ACTION CONTROL --"); ;
                String[] items = Enum.GetNames(typeof(HotbarSlotType));
                ImGui.ListBox("Action Type", ref this.selected_action_type, items, items.Length);
                ImGui.InputInt("Command ID", ref this.selected_action_id);
                
                ImGui.Text($"Action Unlocked: {FFXIVClientStructs.FFXIV.Client.Game.UI.UIState.Instance()->Hotbar.IsActionUnlocked((uint) this.selected_action_id)}");

                if (ImGui.Button("EXECUTE")) {
                    HotbarSlotType selType;
                    HotbarSlotType.TryParse(items[this.selected_action_type], out selType);
                    
                    PluginLog.Debug($"Executing {selType} #{this.selected_action_id}");
                    _plugin.SigHelper.ExecuteHotbarAction(selType, (uint) this.selected_action_id);
                }

            }
            ImGui.End();
        }

        public void DrawSettingsWindow() {
            if (!SettingsVisible) {
                return;
            }
            
            if (ImGui.Begin("XIVDeck Configuration Menu", ref this.settingsVisible)) {
                var wsPort = this._plugin.Configuration.WebSocketPort;
                var safeMode = this._plugin.Configuration.SafeMode;
                var hasLinkedStreamDeck = this._plugin.Configuration.HasLinkedStreamDeckPlugin;

                if (!safeMode) {
                    ImGui.TextColored(ImGuiColors.DalamudRed, "DANGER: SAFE MODE DISABLED! You may be able to " +
                                                              "send illegal commands from your Stream Deck to the " +
                                                              "game.");
                }

                if (!hasLinkedStreamDeck) {
                    ImGui.TextColored(ImGuiColors.DalamudYellow, "A Stream Deck has never been connected to this " +
                                                                 "plugin instance. Please install the plugin as " +
                                                                 "documented in the GitHub repo for this plugin.");
                }

                if (ImGui.InputInt("WebSocket Port", ref wsPort, 0)) {
                    if (wsPort < 1024) wsPort = 1024;
                    if (wsPort > 59999) wsPort = 59999;

                    this._plugin.Configuration.WebSocketPort = wsPort;
                    this._plugin.Configuration.Save();
                }
                ImGui.TextWrapped("Port number must be between 1024-59999. Changes to the port number require the plugin " +
                           "be reloaded to take effect. Note that reconfiguration of the Stream Deck plugin will " +
                           "also be required if the WebSocket port were to be changed.");
                ImGui.TextWrapped("Default port: 37984");
            }
            ImGui.End();
        }
    }
}
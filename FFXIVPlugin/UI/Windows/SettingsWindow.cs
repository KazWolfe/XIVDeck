using System.Numerics;
using Dalamud.Interface;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Components;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using XIVDeck.FFXIVPlugin.Resources.Localization;
using XIVDeck.FFXIVPlugin.UI.Windows.Nags;
using XIVDeck.FFXIVPlugin.Utils;

namespace XIVDeck.FFXIVPlugin.UI.Windows;

public class SettingsWindow : Window {
    public const string WindowKey = "###xivDeckSettingsWindow";

    private readonly XIVDeckPlugin _plugin = XIVDeckPlugin.Instance;

    // settings
    private int _websocketPort;
    private bool _safeMode = true;
    private bool _usePenumbraIPC;
    private bool _useMIconIcons;
    private bool _listenOnAllInterfaces;

    public SettingsWindow(bool forceMainWindow = true) :
        base(WindowKey, ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoCollapse, forceMainWindow) {
        
        this.SizeCondition = ImGuiCond.FirstUseEver;
        this.SizeConstraints = new WindowSizeConstraints {
            MinimumSize = new Vector2(350, 250),
            MaximumSize = new Vector2(450, 400)
        };
        this.Size = this.SizeConstraints.Value.MinimumSize;
        
        this.IsOpen = true;
    }

    public override void OnOpen() {
        this._websocketPort = this._plugin.Configuration.WebSocketPort;
        this._safeMode = this._plugin.Configuration.SafeMode;

        this._listenOnAllInterfaces = this._plugin.Configuration.ListenOnAllInterfaces;
        
        // setting flags
        this._usePenumbraIPC = this._plugin.Configuration.UsePenumbraIPC;

        // experimental flags
        this._useMIconIcons = this._plugin.Configuration.UseMIconIcons;
    }

    public override void OnClose() {
        this._plugin.WindowSystem.RemoveWindow(this);
    }

    public override void Draw() {
        var windowSize = ImGui.GetContentRegionAvail();
        this.WindowName = UIStrings.SettingsWindow_Title + WindowKey;

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

        ImGui.PushItemWidth(80);

        if (ImGui.InputInt(UIStrings.SettingsWindow_APIPort, ref this._websocketPort, 0)) {
            if (this._websocketPort < 1024) this._websocketPort = 1024;
            if (this._websocketPort > 59999) this._websocketPort = 59999;
        }

        ImGui.PopItemWidth();
        ImGuiComponents.HelpMarker(string.Format(UIStrings.SettingsWindow_APIPort_Help, 37984, 1024, 59999));

        if (this._plugin.Configuration.ListenOnAllInterfaces) {
            ImGui.Checkbox(UIStrings.SettingsWindow_ListenOnNetwork, ref this._listenOnAllInterfaces);
            ImGui.PushTextWrapPos();
            ImGui.TextWrapped(string.Format(UIStrings.SettingsWindow_ListenIP, "0.0.0.0"));
            ImGui.TextColored(ImGuiColors.DalamudYellow, UIStrings.SettingsWindow_XIVDeckPortOpen);
            ImGui.PopTextWrapPos();
        } else {
            ImGui.TextWrapped(string.Format(UIStrings.SettingsWindow_ListenIP, "127.0.0.1"));
        }

        ImGui.Spacing();
        ImGui.Checkbox(UIStrings.SettingsWindow_EnablePenumbraIPC, ref this._usePenumbraIPC);
        ImGuiComponents.HelpMarker(UIStrings.SettingsWindow_EnablePenumbraIPC_Help);
        
        ImGui.Checkbox(UIStrings.SettingsWindow_Experiment_MIcon, ref this._useMIconIcons);
        ImGuiComponents.HelpMarker(UIStrings.SettingsWindow_UseMIcon_Help);
        
        ImGui.Dummy(new Vector2(0, 10));

        ImGui.EndChild();

        /* FOOTER */
        ImGui.Separator();

        if (ImGui.Button(UIStrings.SettingsWindow_GitHubLink)) UiUtil.OpenXIVDeckGitHub();
        
#if DEBUG
        ImGui.SameLine();
        if (ImGui.Button("Debug")) {
            DebugWindow.InitializeWindow();
        }
#endif

        var applyText = UIStrings.SettingsWindow_ApplyButton;
        var applyButtonSize = ImGuiHelpers.GetButtonSize(applyText);

        ImGui.SameLine(windowSize.X - applyButtonSize.X - 5);
        if (ImGui.Button(applyText)) {
            this.SaveSettings();
        }
    }

    private void SaveSettings() {
        if (this._websocketPort != this._plugin.Configuration.WebSocketPort) {
            this._plugin.Configuration.WebSocketPort = this._websocketPort;
            this._plugin.Configuration.HasLinkedStreamDeckPlugin = false;
            
            NagWindow.CloseAllNags();
            SetupNag.Show();
        }

        this._plugin.Configuration.UsePenumbraIPC = this._usePenumbraIPC;
        this._plugin.Configuration.UseMIconIcons = this._useMIconIcons;

        this._plugin.Configuration.ListenOnAllInterfaces = this._listenOnAllInterfaces;
        
        this._plugin.Configuration.Save();

        // initialize regardless of change(s) so that we can easily restart the server when necessary
        this._plugin.InitializeWebServer();
    }
}
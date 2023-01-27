using System.Numerics;
using Dalamud.Interface.Colors;
using ImGuiNET;
using XIVDeck.FFXIVPlugin.Resources.Localization;
using XIVDeck.FFXIVPlugin.Utils;

namespace XIVDeck.FFXIVPlugin.UI.Windows.Nags; 

public class SetupNag : NagWindow {
    private static SetupNag? _instance;

    internal static void Show() {
        _instance ??= new SetupNag();
        _instance.IsOpen = true;
    }

    internal static void Hide() {
        if (_instance == null) return;
        _instance.IsOpen = false;
    }

    public SetupNag() : base("sdPluginNotInstalled", 400, 325) { }

    protected override void _internalDraw() {
        ImGui.PushStyleColor(ImGuiCol.Text, ImGuiColors.DalamudYellow);
        ImGui.Text(UIStrings.SetupNag_Headline);
        ImGui.PopStyleColor();
        
        ImGui.Separator();
        
        ImGui.Text(UIStrings.SetupNag_ResolutionHelp);
        
        if (ImGui.Button(UIStrings.Nag_OpenGithubDownloadButton)) {
            UiUtil.OpenXIVDeckGitHub($"/releases/tag/v{VersionUtils.GetCurrentMajMinBuild()}");
        }
            
        // spacer, but bigger
        ImGui.Dummy(new Vector2(0, 15));
        
        ImGui.Text(UIStrings.SetupNag_AlreadyInstalledHelp);
        
        ImGui.Text(string.Format(UIStrings.SetupNag_CurrentPort, XIVDeckPlugin.Instance.Configuration.WebSocketPort));
        
        ImGui.Text(UIStrings.SetupNag_PortChangeHelp);
        if (ImGui.Button(UIStrings.Nag_OpenSettingsButton)) {
            XIVDeckPlugin.Instance.DrawConfigUI();
        }
            
        ImGui.Spacing();
        ImGui.TextColored(ImGuiColors.DalamudGrey, UIStrings.SetupNag_DismissHelp);
    }
}
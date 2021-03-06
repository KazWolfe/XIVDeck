using Dalamud.Interface;
using Dalamud.Interface.Colors;
using ImGuiNET;
using XIVDeck.FFXIVPlugin.Resources.Localization;
using XIVDeck.FFXIVPlugin.Utils;

namespace XIVDeck.FFXIVPlugin.UI.Windows.Nags; 

public class ForcedUpdateNag : NagWindow {
    private static ForcedUpdateNag? _instance;

    public static void Show() {
        _instance ??= new ForcedUpdateNag();
        _instance.IsOpen = true;
    }

    public static void Hide() {
        if (_instance == null) return;
        
        _instance.Dispose();
        _instance = null;
    }

    public ForcedUpdateNag() : base("sdPluginVeryOutdated", 300, 150) { }
    
    protected override void _internalDraw() {
        var windowSize = ImGui.GetWindowContentRegionMax();
        var placeholderButtonSize = ImGuiHelpers.GetButtonSize("placeholder");
        
        ImGui.PushStyleColor(ImGuiCol.Text, ImGuiColors.DalamudRed);
        ImGui.Text(UIStrings.ForcedUpdateNag_Headline);
        ImGui.PopStyleColor();
        
        ImGui.Separator();
        
        ImGui.Text(UIStrings.ForcedUpdateNag_ResolutionHelp);
        

        ImGui.SetCursorPosY(windowSize.Y - placeholderButtonSize.Y);
        if (ImGui.Button(UIStrings.Nag_OpenGithubDownloadButton)) {
            PluginUI.OpenXIVDeckGitHub($"/releases/tag/v{VersionUtils.GetCurrentMajMinBuild()}");
        }
    }
}
using Dalamud.Interface.Colors;
using Dalamud.Interface.Components;
using ImGuiNET;
using XIVDeck.FFXIVPlugin.Resources.Localization;
using XIVDeck.FFXIVPlugin.Utils;

namespace XIVDeck.FFXIVPlugin.UI.Windows.Nags; 

public class ForcedUpdateNag : NagWindow {
    private static ForcedUpdateNag? _instance;

    internal static void Show() {
        _instance ??= new ForcedUpdateNag();
        _instance.IsOpen = true;
    }

    internal static void Hide() {
        if (_instance == null) return;
        _instance.IsOpen = false;
    }

    private string _versionString = VersionUtils.GetCurrentMajMinBuild();

    public ForcedUpdateNag() : base("sdPluginVeryOutdated", 350) { }
    
    protected override void _internalDraw() {
        ImGui.TextColored(ImGuiColors.DalamudRed, UIStrings.ForcedUpdateNag_Headline);
        
        ImGui.Separator();
        
        ImGui.Text(UIStrings.ForcedUpdateNag_ProblemDescription);
        ImGui.Text(UIStrings.ForcedUpdateNag_SupportInfo);
        
        if (ImGui.GetIO().KeyCtrl) {
            if (ImGui.Button(UIStrings.ForcedUpdateNag_BypassButton)) {
                Hide();
            }
            return;
        }
        
        if (ImGui.Button(UIStrings.Nag_OpenGithubDownloadButton)) {
            UiUtil.OpenXIVDeckGitHub($"/releases/tag/v{this._versionString}");
        }
        ImGui.SameLine();
        if (ImGui.Button(UIStrings.ForcedUpdateNag_SupportButton)) {
            Dalamud.Utility.Util.OpenLink(Constants.GoatPlaceDiscord);
        }
        ImGuiComponents.HelpMarker(UIStrings.ForcedUpdateNag_BypassInstructions);
    }
}
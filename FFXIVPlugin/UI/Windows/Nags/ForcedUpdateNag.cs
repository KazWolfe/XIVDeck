using Dalamud.Interface;
using Dalamud.Interface.Colors;
using ImGuiNET;
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
        ImGui.Text("The XIVDeck Stream Deck Plugin is critically out of date and has been disabled.");
        ImGui.PopStyleColor();
        
        ImGui.Separator();
        
        ImGui.Text("Please download and install the latest version of the Stream Deck plugin from the XIVDeck " +
                   "GitHub to continue using XIVDeck.");
        

        ImGui.SetCursorPosY(windowSize.Y - placeholderButtonSize.Y);
        if (ImGui.Button($"Open XIVDeck Download Page")) {
            PluginUI.OpenXIVDeckGitHub($"/releases/tag/v{StringUtils.GetMajMinBuild()}");
        }
    }
}
using Dalamud.Interface;
using Dalamud.Interface.Colors;
using ImGuiNET;
using XIVDeck.FFXIVPlugin.Utils;

namespace XIVDeck.FFXIVPlugin.UI.Windows.Nags;

public class TestingUpdateNag : NagWindow {
    private static TestingUpdateNag? _instance;
    private static bool _dismissed = false;

    public static void Show() {
// #if DEBUG
//         return;
// #endif

        if (_dismissed) return;

        _instance ??= new TestingUpdateNag();
        _instance.IsOpen = true;
    }

    public static void Hide() {
        if (_instance == null) return;

        _instance.Dispose();
        _instance = null;
    }

    public TestingUpdateNag() : base("sdPluginTestVersionMismatch", 400, 250) { }

    protected override void _internalDraw() {
        var windowSize = ImGui.GetWindowContentRegionMax();
        var placeholderButtonSize = ImGuiHelpers.GetButtonSize("placeholder");

        var currentVersion = VersionUtils.GetCurrentMajMinBuild();

        ImGui.PushStyleColor(ImGuiCol.Text, ImGuiColors.DalamudYellow);
        ImGui.Text("Warning: Testing Version Mismatch");
        ImGui.PopStyleColor();

        ImGui.Separator();

        ImGui.Text("A version mismatch was detected between the currently-installed version of the Stream Deck " +
                   "Plugin and the Game Plugin. Certain features may not work properly.");

        ImGui.Text("In order to properly help test XIVDeck releases, you need to be on the correct (matching) " +
                   "version of the Stream Deck Plugin. Please use the below button to download the correct version " +
                   "of the Stream Deck Plugin for your testing build.");

        ImGui.Spacing();

        ImGui.TextColored(ImGuiColors.DalamudGrey, "This message may be dismissed, but will reappear so long as " +
                                                   "test mode is enabled and a version mismatch is detected.");

        ImGui.SetCursorPosY(windowSize.Y - placeholderButtonSize.Y);
        if (ImGui.Button("Ignore For Now")) {
            _dismissed = true;
            Hide();
        }

        ImGui.SameLine();
        if (ImGui.Button($"Download v{currentVersion}")) {
            UiUtil.OpenXIVDeckGitHub($"/releases/tag/v{currentVersion}");
        }
    }
}
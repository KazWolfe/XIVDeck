using Dalamud.Interface;
using Dalamud.Interface.Colors;
using ImGuiNET;
using XIVDeck.FFXIVPlugin.Resources.Localization;
using XIVDeck.FFXIVPlugin.Utils;

namespace XIVDeck.FFXIVPlugin.UI.Windows.Nags;

public class TestingUpdateNag : NagWindow {
    private static TestingUpdateNag? _instance;
    private static bool _dismissed = false;

    internal static void Show() {
        if (_dismissed) return;

        _instance ??= new TestingUpdateNag();
        _instance.IsOpen = true;
    }

    internal static void Hide() {
        if (_instance == null) return;
        _instance.IsOpen = false;
    }

    public TestingUpdateNag() : base("sdPluginTestVersionMismatch", 400) { }

    protected override void _internalDraw() {
        var currentVersion = VersionUtils.GetCurrentMajMinBuild();

        ImGui.PushStyleColor(ImGuiCol.Text, ImGuiColors.DalamudYellow);
        ImGui.Text(UIStrings.TestingUpdateNag_Headline);
        ImGui.PopStyleColor();

        ImGui.Separator();

        ImGui.Text(UIStrings.TestingUpdateNag_MismatchDetectedText);
        ImGui.Text(UIStrings.TestingUpdateNag_PleaseTestProperly);

        ImGui.Spacing();

        ImGui.TextColored(ImGuiColors.DalamudGrey, UIStrings.TestingUpdateNag_DismissHelp);

        if (ImGui.Button(UIStrings.TestingUpdateNag_IgnoreButton)) {
            _dismissed = true;
            Hide();
        }

        ImGui.SameLine();

        var buttonText = string.Format(UIStrings.TestingUpdateNag_DownloadButton, currentVersion);
        var buttonSize = ImGuiHelpers.GetButtonSize(buttonText);
        buttonSize.X *= 2;
        ImGui.SetCursorPosX(ImGui.GetWindowWidth() - buttonSize.X - ImGui.GetStyle().ItemSpacing.X);
        if (ImGui.Button(buttonText, buttonSize)) {
            UiUtil.OpenXIVDeckGitHub($"/releases/tag/v{currentVersion}");
        }
    }
}
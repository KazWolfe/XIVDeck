using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Colors;
using XIVDeck.FFXIVPlugin.Base;
using XIVDeck.FFXIVPlugin.Resources.Localization;

namespace XIVDeck.FFXIVPlugin.UI.Windows.Nags;

// Unused for now, but may come back for some purpose in the future.
public class PortInUseNag() : NagWindow("sdPortInUse", 350) {
    private static PortInUseNag? _instance;

    internal static void Show() {
        _instance ??= new PortInUseNag();
        _instance.IsOpen = true;
    }

    internal static void Hide() {
        if (_instance == null) return;
        _instance.IsOpen = false;
    }

    private readonly PluginConfig _pluginConfig = XIVDeckPlugin.Instance.Configuration;

    protected override void _internalDraw() {
        ImGui.PushStyleColor(ImGuiCol.Text, ImGuiColors.DalamudYellow);
        ImGui.Text(UIStrings.PortInUseNag_Title);
        ImGui.PopStyleColor();

        ImGui.Separator();

        ImGui.Text(string.Format(UIStrings.PortInUseNag_PortAlreadyInUse, this._pluginConfig.WebSocketPort));

        ImGui.Text(UIStrings.PortInUseNag_ResolutionInstructions);

        ImGui.Spacing();

        if (ImGui.Button(UIStrings.Nag_OpenSettingsButton)) {
            XIVDeckPlugin.Instance.DrawConfigUI();
        }

        ImGui.SameLine();
        if (ImGui.Button(UIStrings.PortInUseNag_IgnoreForNow)) {
            Hide();
        }
    }

    public override void OnClose() {
        // we allow the port in use nag to be closed.
        Hide();
    }
}

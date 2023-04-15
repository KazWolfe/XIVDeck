using Dalamud.Interface.Colors;
using ImGuiNET;
using XIVDeck.FFXIVPlugin.Resources.Localization;

namespace XIVDeck.FFXIVPlugin.UI.Windows.Nags;

// Unused for now, but may come back for some purpose in the future.
public class PortInUseNag : NagWindow {
    private static PortInUseNag? _instance;

    internal static void Show() {
        _instance ??= new PortInUseNag();
        _instance.IsOpen = true;
    }

    internal static void Hide() {
        if (_instance == null) return;
        _instance.IsOpen = false;
    }

    private PortInUseNag() : base("sdPortInUse") { }

    protected override void _internalDraw() {
        ImGui.PushStyleColor(ImGuiCol.Text, ImGuiColors.DalamudYellow);
        ImGui.Text("The XIVDeck API port is already in use!");
        ImGui.PopStyleColor();

        ImGui.Separator();

        ImGui.Text($"XIVDeck has detected that port {XIVDeckPlugin.Instance.Configuration.WebSocketPort} is " +
                   "already in use by another application.");

        ImGui.Text("In order to use XIVDeck, this port must be changed to a free one. To do this, please choose " +
                   "a new port number in the XIVDeck Game Plugin settings, and enter that same port number in the " +
                   "XIVDeck Stream Deck Plugin settings.");

        if (ImGui.Button(UIStrings.Nag_OpenSettingsButton)) {
            XIVDeckPlugin.Instance.DrawConfigUI();
        }

        ImGui.SameLine();
        if (ImGui.Button("Ignore")) {
            Hide();
        }
    }

    public override void OnClose() {
        // we allow the port in use nag to be closed.
        Hide();
    }
}
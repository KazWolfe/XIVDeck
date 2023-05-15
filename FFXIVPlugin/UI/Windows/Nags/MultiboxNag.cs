using Dalamud.Interface.Colors;
using ImGuiNET;
using XIVDeck.FFXIVPlugin.Base;
using XIVDeck.FFXIVPlugin.Game.Chat;
using XIVDeck.FFXIVPlugin.Resources.Localization;

namespace XIVDeck.FFXIVPlugin.UI.Windows.Nags;

public class MultiboxNag : NagWindow {
    private static MultiboxNag? _instance;

    internal static void Show() {
        // always show chat message, despite nag suppression.
        var chatWarning = ErrorNotifier.BuildPrefixedString(UIStrings.MultiboxNag_ChatWarning);
        DeferredChat.SendOrDeferMessage(chatWarning);

        if (XIVDeckPlugin.Instance.Configuration.SuppressMultiboxNag)
            return;

        _instance ??= new MultiboxNag();
        _instance.IsOpen = true;
    }


    internal static void Hide() {
        if (_instance == null) return;
        _instance.IsOpen = false;
    }

    private readonly PluginConfig _pluginConfig;

    private bool _suppressMultiboxNag;

    private MultiboxNag() : base("multiboxWarning", 350) {
        this._pluginConfig = XIVDeckPlugin.Instance.Configuration;
    }

    public override void PreDraw() {
        this._suppressMultiboxNag = XIVDeckPlugin.Instance.Configuration.SuppressMultiboxNag;
    }

    protected override void _internalDraw() {
        ImGui.PushStyleColor(ImGuiCol.Text, ImGuiColors.DalamudYellow);
        ImGui.Text(UIStrings.PortInUseNag_Title);
        ImGui.PopStyleColor();

        ImGui.Separator();

        ImGui.Text(UIStrings.MultiboxNag_MultiboxingDetected);

        ImGui.Text(UIStrings.MultiboxNag_MultiboxingIsUnsupported);

        ImGui.Spacing();

        if (ImGui.Checkbox(UIStrings.MultiboxNag_SuppressWarning, ref this._suppressMultiboxNag)) {
            this._pluginConfig.SuppressMultiboxNag = this._suppressMultiboxNag;
        }

        ImGui.Spacing();

        if (ImGui.Button(UIStrings.MultiboxNag_CloseButton)) {
            Hide();
        }
    }

    public override void OnClose() {
        Hide();
    }
}
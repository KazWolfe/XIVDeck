using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Components;
using Dalamud.Utility;
using XIVDeck.FFXIVPlugin.Config;
using XIVDeck.FFXIVPlugin.IoC;
using XIVDeck.FFXIVPlugin.Resources.Localization;
using XIVDeck.FFXIVPlugin.RpcServer;
using XIVDeck.FFXIVPlugin.Utils;

namespace XIVDeck.FFXIVPlugin.UI.Windows.Nags;

[Service(ServiceFlags.Transient)]
public class SetupNag(UIManager uiManager, PluginConfig pluginConfig, TransportManager transportManager)
    : NagWindow(uiManager, "sdPluginNotInstalled", 400) {

    private int? WebSocketPortToAdvertise =>
        transportManager.RunningTransportType == TransportType.WebSocket
            ? ((WebSocketTransportConfig) pluginConfig.GetTransport(TransportType.WebSocket)).Port
            : null;

    protected override void _internalDraw() {
        ImGui.PushStyleColor(ImGuiCol.Text, ImGuiColors.DalamudYellow);
        ImGui.Text(UIStrings.SetupNag_Headline);
        ImGui.PopStyleColor();

        ImGui.Separator();

        ImGui.Text(UIStrings.SetupNag_ResolutionHelp);

        if (ImGui.Button(UIStrings.Common_OpenGitHubDownloadButton)) {
            UiUtil.OpenXIVDeckGitHub($"/releases/tag/v{VersionUtils.GetCurrentMajMinBuild()}");
        }

        ImGui.Spacing();

        if (ImGui.CollapsingHeader(UIStrings.SetupNag_HowInstall)) {
            ImGui.Indent(10);

            ImGui.Text(UIStrings.SetupNag_HowInstall_Requirements);

            ImGui.Indent(10);
            ImGui.TextUnformatted(UIStrings.SetupNag_HowInstall_Steps);
            ImGui.Unindent(10);

            ImGui.TextUnformatted(UIStrings.SetupNag_HowInstall_OtherInfo);

            ImGui.Unindent(10);
        }

        if (ImGui.CollapsingHeader(UIStrings.SetupNag_AlreadyInstalled)) {
            ImGui.Indent(10);

            ImGui.Text(UIStrings.SetupNag_AlreadyInstalled_Checklist);

            if (this.WebSocketPortToAdvertise is {} port) {
                ImGui.Spacing();
                ImGui.TextColored(ImGuiColors.DalamudYellow,
                    string.Format(UIStrings.SetupNag_AlreadyInstalled_WebSocketHelp, port));
            }

            ImGui.Spacing();
            ImGui.Text(UIStrings.SetupNag_AlreadyInstalled_ChangeConnectionHelp);

            if (ImGui.Button(UIStrings.Common_OpenSettingsButton)) {
                this.UiManager.ShowOrFocus<SettingsWindow>();
            }

            ImGui.Unindent(10);
        }

        ImGui.Spacing();
        ImGui.TextColored(ImGuiColors.DalamudGrey, UIStrings.SetupNag_DismissHelp);

        ImGui.AlignTextToFramePadding();
        ImGui.TextColored(ImGuiColors.DalamudGrey, UIStrings.Common_SupportInfo);
        ImGui.SameLine();
        if (ImGuiComponents.IconButton(FontAwesomeIcon.Headset)) {
            Util.OpenLink(Constants.GoatPlaceDiscord);
        }
        if (ImGui.IsItemHovered()) ImGui.SetTooltip(UIStrings.SetupNag_JoinDiscord);

        if (ImGui.GetIO().KeyCtrl) {
            if (ImGui.Button(UIStrings.SetupNag_BypassButton)) {
                this.IsOpen = false;
            }
            ImGuiComponents.HelpMarker(UIStrings.SetupNag_MessageWillReturn);
        } else {
            ImGui.TextColored(ImGuiColors.DalamudGrey2, UIStrings.SetupNag_BypassHint);
        }
    }
}

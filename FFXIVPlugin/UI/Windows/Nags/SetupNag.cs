using Dalamud.Interface;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Components;
using Dalamud.Utility;
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

    public SetupNag() : base("sdPluginNotInstalled", 400) { }

    protected override void _internalDraw() {
        ImGui.PushStyleColor(ImGuiCol.Text, ImGuiColors.DalamudYellow);
        ImGui.Text(UIStrings.SetupNag_Headline);
        ImGui.PopStyleColor();
        
        ImGui.Separator();
        
        ImGui.Text(UIStrings.SetupNag_ResolutionHelp);
        
        if (ImGui.Button(UIStrings.Nag_OpenGithubDownloadButton)) {
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
            
            ImGui.Text(string.Format(UIStrings.SetupNag_AlreadyInstalledHelp,
                XIVDeckPlugin.Instance.Configuration.WebSocketPort));
        
            ImGui.Text(UIStrings.SetupNag_PortChangeHelp);
        
            if (ImGui.Button(UIStrings.Nag_OpenSettingsButton)) {
                XIVDeckPlugin.Instance.DrawConfigUI();
            }
            
            ImGui.Unindent(10);
        }

        ImGui.Spacing();
        ImGui.TextColored(ImGuiColors.DalamudGrey, UIStrings.SetupNag_DismissHelp);
        
        ImGui.AlignTextToFramePadding();
        ImGui.TextColored(ImGuiColors.DalamudGrey, UIStrings.ForcedUpdateNag_SupportInfo);
        ImGui.SameLine();
        if (ImGuiComponents.IconButton(FontAwesomeIcon.Headset)) {
            Util.OpenLink(Constants.GoatPlaceDiscord);
        }
        if (ImGui.IsItemHovered()) ImGui.SetTooltip(UIStrings.SetupNag_JoinDiscord); 
        
        if (ImGui.GetIO().KeyCtrl) {
            if (ImGui.Button(UIStrings.SetupNag_BypassButton)) {
                Hide();
            }
            ImGuiComponents.HelpMarker(UIStrings.SetupNag_MessageWillReturn);
        } else {
            ImGui.TextColored(ImGuiColors.DalamudGrey2, UIStrings.SetupNag_BypassHint);
        }
    }
}
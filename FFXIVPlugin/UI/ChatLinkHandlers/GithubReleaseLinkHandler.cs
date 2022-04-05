using Dalamud.Game.Text.SeStringHandling;
using XIVDeck.FFXIVPlugin.Utils;

namespace XIVDeck.FFXIVPlugin.UI.ChatLinkHandlers; 

[ChatLinkHandler(LinkCode.GetGithubReleaseLink)]
public class GithubReleaseLinkHandler : IChatLinkHandler {
    public void Handle(uint opcode, SeString clickedString) {
        PluginUI.OpenXIVDeckGitHub($"/releases/tag/v{VersionUtils.GetCurrentMajMinBuild()}");
    }
}
using System;
using Dalamud.Game.Text.SeStringHandling;
using XIVDeck.FFXIVPlugin.Utils;

namespace XIVDeck.FFXIVPlugin.UI.ChatLinkHandlers;

[ChatLinkHandler(LinkCode.GetGithubReleaseLink)]
public class GithubReleaseLinkHandler : IChatLinkHandler {
    public void Handle(Guid commandId, SeString payload) {
        UiUtil.OpenXIVDeckGitHub($"/releases/tag/v{VersionUtils.GetCurrentMajMinBuild()}");
    }
}

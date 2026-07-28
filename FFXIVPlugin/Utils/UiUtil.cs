using Dalamud.Utility;

namespace XIVDeck.FFXIVPlugin.Utils;

public static class UiUtil {
    public static void OpenXIVDeckGitHub(string? extra = null) {
        Util.OpenLink(Constants.GithubUrl + extra);
    }
}

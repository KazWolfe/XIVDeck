namespace XIVDeck.FFXIVPlugin.Utils; 

public static class UiUtil {
    public static void OpenXIVDeckGitHub(string? extra = null) {
        Dalamud.Utility.Util.OpenLink(Constants.GithubUrl + extra);
    }
}
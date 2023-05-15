using System.Diagnostics;

namespace XIVDeck.FFXIVPlugin.Utils; 

public static class UiUtil {
    public static void OpenXIVDeckGitHub(string? extra = null) {
        Dalamud.Utility.Util.OpenLink(Constants.GithubUrl + extra);
    }

    internal static bool IsMultiboxing() {
        return Process.GetProcessesByName("ffxiv_dx11").Length > 1;
    }
}
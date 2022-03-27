using System.Diagnostics;

namespace XIVDeck.FFXIVPlugin.UI;

internal static class PluginUI {
    public static void OpenXIVDeckGitHub(string? extra = null) {
        Process.Start(new ProcessStartInfo {
            FileName = Constants.GithubUrl + extra,
            UseShellExecute = true,
        });
    }
}
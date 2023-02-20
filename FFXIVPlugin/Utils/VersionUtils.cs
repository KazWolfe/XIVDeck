using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using XIVDeck.FFXIVPlugin.Game.Chat;
using XIVDeck.FFXIVPlugin.Resources.Localization;
using XIVDeck.FFXIVPlugin.UI;

namespace XIVDeck.FFXIVPlugin.Utils;

public static class VersionUtils {
    public static string GetCurrentMajMinBuild() {
        return Assembly.GetExecutingAssembly().GetName().Version!.GetMajMinBuild();
    }

    public static string GetMajMinBuild(this Version version) {
        return $"{version.Major}.{version.Minor}.{version.Build}";
    }

    public static Version StripRevision(this Version version) {
        return new Version(version.Major, version.Minor, version.Build);
    }

    public static SeString GenerateUpdateNagString(Version xivPluginVersion) {
        // This method is terrible and ugly and I hate it, but is basically necessary because string interpolation with
        // SeStrings is *absolute total pain*. I tried to make it localization friendly but that's not particularly 
        // easy, sadly.
        
        var versionText = xivPluginVersion.GetMajMinBuild();
        var versionHighlight = new SeStringBuilder()
            .Add(ChatLinkWiring.GetPayload(LinkCode.GetGithubReleaseLink))
            .AddUiForeground(string.Format(UIStrings.VersionUtils_UpdateAlert_Link, "\xE0BB", versionText), 32)
            .Add(RawPayload.LinkTerminator)
            .Build();

        var outer = UIStrings.VersionUtils_UpdateAlert.Split("{0}", 2);
        var components = outer.Select(segment => (SeString) segment).ToList();
        components.Insert(1, versionHighlight); 

        return new SeStringBuilder()
            .Append(ErrorNotifier.BuildPrefixedString(""))
            .AddText(outer[0])
            .Append(versionHighlight)
            .AddText(outer[1])
            .Build();
    }
}
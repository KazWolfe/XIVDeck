using System;
using System.Reflection;

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
}
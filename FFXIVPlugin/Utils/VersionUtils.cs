using System;
using System.Collections.Generic;
using System.Reflection;

namespace XIVDeck.FFXIVPlugin.Utils;

public static class VersionUtils {
    public static string GetCurrentMajMinBuild() {
        return Assembly.GetExecutingAssembly().GetName().Version!.GetMajMinBuild();
    }

    public static string GetMajMinBuild(this Version version) {
        return $"{version.Major}.{version.Minor}.{version.Build}";
    }

    /// <summary>
    /// Compare two versions by Maj.Min.Build only, ignoring revisions.
    /// </summary>
    /// <param name="left">The version to check against.</param>
    /// <param name="right">The version to compare relative to.</param>
    /// <returns>Returns true if the passed version is newer than the base version.</returns>
    public static bool IsOlderThan(this Version left, Version right) {
        if (left.Major < right.Major) return true;
        if (left.Major > right.Major) return false;

        if (left.Minor < right.Minor) return true;
        if (left.Minor > right.Minor) return false;

        return left.Build < right.Build;
    }
}
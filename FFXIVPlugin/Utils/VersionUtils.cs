using System;
using System.Reflection;

namespace XIVDeck.FFXIVPlugin.Utils;

public static class VersionUtils {
    public static Version GetCurrentVersion() {
        return Assembly.GetExecutingAssembly().GetName().Version!.StripRevision();
    }

    public static string GetCurrentMajMinBuild() {
        return GetCurrentVersion().GetMajMinBuild();
    }

    public static string GetMajMinBuild(this Version version) {
        return $"{version.Major}.{version.Minor}.{version.Build}";
    }

    public static Version StripRevision(this Version version) {
        return new Version(version.Major, version.Minor, version.Build);
    }

    /// <summary>
    /// The most significant version component, which is the one that breaks compatibility: the major version, or
    /// <c>0.minor</c> while still on 0.x.
    /// </summary>
    public static string GetCompatibilityLevel(this Version version) {
        return version.Major == 0 ? $"0.{version.Minor}" : $"{version.Major}";
    }

    public static bool IsCompatibleWith(this Version version, Version other) {
        return version.GetCompatibilityLevel() == other.GetCompatibilityLevel();
    }
}

using System;
using System.Reflection;

namespace XIVDeck.FFXIVPlugin.Utils; 

public static class StringUtils {
    public static string GetMajMinBuild() {
        return GetMajMinBuild(Assembly.GetExecutingAssembly().GetName().Version!);
    }
    
    public static string GetMajMinBuild(Version version) {
        return $"{version.Major}.{version.Minor}.{version.Build}";
    } 
}
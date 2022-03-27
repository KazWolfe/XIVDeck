using System;
using System.Reflection;

namespace XIVDeck.FFXIVPlugin.Utils; 

public static class StringUtils {
    public static string GetMajMinRev() {
        return GetMajMinRev(Assembly.GetExecutingAssembly().GetName().Version!);
    }
    
    public static string GetMajMinRev(Version version) {
        return $"{version.Major}.{version.Minor}.{version.Revision}";
    } 
}
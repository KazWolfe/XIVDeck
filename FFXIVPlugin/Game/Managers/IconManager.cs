using System;
using System.IO;

using Lumina.Data.Files;
using XIVDeck.FFXIVPlugin.Base;
using XIVDeck.FFXIVPlugin.IPC.Subscribers;
using XIVDeck.FFXIVPlugin.Utils;

namespace XIVDeck.FFXIVPlugin.Game.Managers; 

// borrowed from https://github.com/Caraxi/RemindMe/blob/master/IconManager.cs
public static class IconManager {
    private const string IconFileFormat = "ui/icon/{0:D3}000/{1}{2:D6}{3}.tex";

    private static string GetIconPath(string lang, int iconId, bool hq = false, bool highres = false, bool forceOriginal = false) {
        var path = string.Format(IconFileFormat, 
            iconId / 1000, (hq ? "hq/" : "") + lang, iconId, highres ? "_hr1" : "");

        if (PenumbraIPC.Instance is {Enabled: true} && !forceOriginal && XIVDeckPlugin.Instance.Configuration.UsePenumbraIPC)
            path = PenumbraIPC.Instance.ResolvePenumbraPath(path);

        return path;
    }

    public static TexFile? GetIcon(string lang, int iconId, bool hq = false, bool highres = false) {
        TexFile? texFile;
        
        if (lang.Length > 0 && !lang.EndsWith("/"))
            lang += "/";
        
        var texPath = GetIconPath(lang, iconId, hq, true);

        if (Path.IsPathRooted(texPath)) {
            Injections.PluginLog.Verbose($"Using on-disk asset {texPath}");
            texFile = Injections.DataManager.GameData.GetFileFromDisk<TexFile>(texPath);
        } else {
            texFile = Injections.DataManager.GetFile<TexFile>(texPath);
        }

        // recursion steps:
        // - attempt to get the high-res file exactly as described.
        //   - attempt to get the high-res file without language
        //     - attempt to get the low-res file without language
        //   - attempt to get the low-res file exactly as described
        //     - attempt to get the low-res file without language
        // - give up and return null
        switch (texFile) {
            case null when lang.Length > 0:
                Injections.PluginLog.Debug($"Couldn't get lang-specific icon for {texPath}, falling back to no-lang");
                return GetIcon(string.Empty, iconId, hq, true);
            case null when highres:
                Injections.PluginLog.Debug($"Couldn't get highres icon for {texPath}, falling back to lowres");
                return GetIcon(lang, iconId, hq);
            default:
                return texFile;
        }
    }
    
    public static string GetIconAsPngString(int iconId, bool hq = false) {
        var icon = GetIcon("", iconId, hq, true) ?? GetIcon("", 0, hq, true)!;

        return "data:image/png;base64," + Convert.ToBase64String(icon.GetImage().ConvertToPng());
    }
}
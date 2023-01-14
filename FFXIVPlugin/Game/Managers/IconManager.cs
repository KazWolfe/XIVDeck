using System;
using Dalamud.Logging;
using Lumina.Data.Files;
using Lumina.Data.Parsing.Uld;
using XIVDeck.FFXIVPlugin.Base;
using XIVDeck.FFXIVPlugin.IPC.Subscribers;
using XIVDeck.FFXIVPlugin.Utils;

namespace XIVDeck.FFXIVPlugin.Game.Managers;

// borrowed from https://github.com/Caraxi/RemindMe/blob/master/IconManager.cs
public static class IconManager {
    private const string IconFileFormat = "ui/icon/{0:D3}000/{1}{2:D6}{3}.tex";

    private static string GetIconPath(string lang, int iconId, bool hq = false, bool highres = false) {
        return string.Format(IconFileFormat, iconId / 1000, (hq ? "hq/" : "") + lang, iconId, highres ? "_hr1" : "");
    }

    public static TexFile? GetTexture(string texPath, bool forceOriginal = false) {
        TexFile? texFile;

        if (PenumbraIPC.Instance is {Enabled: true} && XIVDeckPlugin.Instance.Configuration.UsePenumbraIPC &&
            !forceOriginal)
            texPath = PenumbraIPC.Instance.ResolvePenumbraPath(texPath);

        if (texPath.Substring(1, 2) == ":\\") {
            PluginLog.Verbose($"Using on-disk asset {texPath}");
            texFile = Injections.DataManager.GameData.GetFileFromDisk<TexFile>(texPath);
        } else {
            texFile = Injections.DataManager.GetFile<TexFile>(texPath);
        }

        return texFile;
    }


    public static TexFile? GetIcon(string lang, int iconId, bool hq = false, bool highres = false) {
        if (lang.Length > 0 && !lang.EndsWith("/"))
            lang += "/";

        var texPath = GetIconPath(lang, iconId, hq, true);
        var texFile = GetTexture(texPath);

        // recursion steps:
        // - attempt to get the high-res file exactly as described.
        //   - attempt to get the high-res file without language
        //     - attempt to get the low-res file without language
        //   - attempt to get the low-res file exactly as described
        //     - attempt to get the low-res file without language
        // - give up and return null
        switch (texFile) {
            case null when lang.Length > 0:
                PluginLog.Debug($"Couldn't get lang-specific icon for {texPath}, falling back to no-lang");
                return GetIcon(string.Empty, iconId, hq, true);
            case null when highres:
                PluginLog.Debug($"Couldn't get highres icon for {texPath}, falling back to lowres");
                return GetIcon(lang, iconId, hq);
            default:
                return texFile;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="uldName"></param>
    /// <param name="partId"></param>
    /// <param name="subPartId"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException">Thrown when the requested ULD does not exist.</exception>
    /// <exception cref="IndexOutOfRangeException">Thrown when partID/subPartID is not found in the ULD.</exception>
    public static (int X, int Y, int W, int H) GetUldRect(string uldName, int partId, int subPartId) {
        var uldFile = Injections.DataManager.GetFile<UldFile>($"ui/uld/{uldName}.uld");
        if (uldFile is null) {
            throw new ArgumentException($"The ULD {uldName} does not exist.");
        }
        
        var partsData = uldFile.Parts[partId];
        var part = partsData.Parts[subPartId];

        return (part.U, part.V, part.W, part.H);
    }

    public static string GetIconAsPngString(int iconId, bool hq = false) {
        var icon = GetIcon("", iconId, hq, true) ?? GetIcon("", 0, hq, true)!;

        return "data:image/png;base64," + Convert.ToBase64String(icon.GetImage().ConvertToPng());
    }
}
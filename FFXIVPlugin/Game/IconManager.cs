using System;
using System.IO;
using Dalamud.Logging;
using Lumina.Data.Files;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using XIVDeck.FFXIVPlugin.Base;
using XIVDeck.FFXIVPlugin.IPC.Subscribers;

namespace XIVDeck.FFXIVPlugin.Game; 

// borrowed from https://github.com/Caraxi/RemindMe/blob/master/IconManager.cs
public class IconManager {
    private const string IconFileFormat = "ui/icon/{0:D3}000/{1}{2:D6}{3}.tex";

    public static string GetIconPath(string lang, int iconId, bool hq = false, bool highres = false, bool forceOriginal = false) {
        var path = string.Format(IconFileFormat, 
            iconId / 1000, (hq ? "hq/" : "") + lang, iconId, highres ? "_hr1" : "");

        if (PenumbraIPC.Instance is {Enabled: true} && !forceOriginal && XIVDeckPlugin.Instance.Configuration.UsePenumbraIPC)
            path = PenumbraIPC.Instance.ResolvePenumbraPath(path);

        return path;
    }

    public TexFile? GetIcon(string lang, int iconId, bool hq = false, bool highres = false) {
        TexFile? texFile;
        
        if (lang.Length > 0 && !lang.EndsWith("/"))
            lang += "/";
        
        var texPath = GetIconPath(lang, iconId, hq, true);

        if (texPath.Substring(1, 2) == ":\\") {
            PluginLog.Verbose($"Using on-disk asset {texPath}");
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
                PluginLog.Debug($"Couldn't get lang-specific icon for {texPath}, falling back to no-lang");
                return this.GetIcon(string.Empty, iconId, hq, true);
            case null when highres:
                PluginLog.Debug($"Couldn't get highres icon for {texPath}, falling back to lowres");
                return this.GetIcon(lang, iconId, hq);
            default:
                return texFile;
        }
    }

    private static Image GetImage(TexFile tex) {
        return Image.LoadPixelData<Bgra32>(tex.ImageData, tex.Header.Width, tex.Header.Height);
    }

    public byte[] GetIconAsPng(int iconId, bool hq = false) {
        var icon = this.GetIcon("", iconId, hq, true) ?? this.GetIcon("", 0, hq, true)!;

        var image = GetImage(icon);

        using var stream = new MemoryStream();
        image.SaveAsPng(stream);
        return stream.ToArray();
    }

    public string GetIconAsPngString(int iconId, bool hq = false) {
        var pngBytes = this.GetIconAsPng(iconId, hq);

        return "data:image/png;base64," + Convert.ToBase64String(pngBytes);
    }
}
using System;
using System.IO;
using Dalamud.Plugin.Services;
using Lumina.Data.Files;
using Serilog;
using XIVDeck.FFXIVPlugin.IoC;
using XIVDeck.FFXIVPlugin.Utils;

namespace XIVDeck.FFXIVPlugin.Game.Managers;

// borrowed from https://github.com/Caraxi/RemindMe/blob/master/IconManager.cs
[Service(ServiceFlags.Singleton)]
public class IconManager(ILogger log, IDataManager dataManager, ITextureSubstitutionProvider tsp) {
    private const string IconFileFormat = "ui/icon/{0:D3}000/{1}{2:D6}{3}.tex";

    public event EventHandler? IconsInvalidated;

    // ToDo: Not called yet. Hook up to texture substitution changes (e.g. Penumbra redraws) once we can detect them.
    public void InvalidateIcons() {
        log.Debug("Icons invalidated, asking clients to clear their icon caches");
        this.IconsInvalidated?.InvokeSafely(this, EventArgs.Empty);
    }

    private string GetIconPath(string lang, int iconId, bool highres = false) {
        var useHqIcon = false;

        if (iconId > 1_000_000) {
            useHqIcon = true;
            iconId -= 1_000_000;
        }

        var path = string.Format(IconFileFormat,
            iconId / 1000, (useHqIcon ? "hq/" : "") + lang, iconId, highres ? "_hr1" : "");

        return tsp.GetSubstitutedPath(path);
    }

    public TexFile? GetIcon(string lang, int iconId, bool highres = false) {
        if (lang.Length > 0 && !lang.EndsWith('/'))
            lang += "/";

        string[] langs = lang.Length > 0 ? [lang, string.Empty] : [string.Empty];
        bool[] resolutions = highres ? [true, false] : [false];

        foreach (var candidateLang in langs) {
            foreach (var candidateHighres in resolutions) {
                var texPath = this.GetIconPath(candidateLang, iconId, candidateHighres);
                var texFile = this.LoadTexFile(texPath);

                if (texFile != null) return texFile;

                log.Debug("Icon {TexPath} not found, trying next candidate", texPath);
            }
        }

        return null;
    }

    private TexFile? LoadTexFile(string texPath) {
        if (!Path.IsPathRooted(texPath)) {
            return dataManager.GetFile<TexFile>(texPath);
        }

        log.Verbose("Using on-disk asset {TexPath}", texPath);
        return dataManager.GameData.GetFileFromDisk<TexFile>(texPath);
    }
}

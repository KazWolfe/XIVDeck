using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Dalamud;
using Dalamud.Logging;
using Dalamud.Plugin;
using Dalamud.Utility;
using ImGuiScene;
using Lumina.Data.Files;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using XIVDeck.FFXIVPlugin.Base;

namespace XIVDeck.FFXIVPlugin.Utils {
    
    // borrowed from https://github.com/Caraxi/RemindMe/blob/master/IconManager.cs
    public class IconManager : IDisposable {

        private readonly DalamudPluginInterface _pluginInterface;
        private bool _disposed;
        private readonly Dictionary<(int, bool), TextureWrap?> _iconTextures = new();

        public IconManager(DalamudPluginInterface pluginInterface) {
            this._pluginInterface = pluginInterface;
        }
        
        public void Dispose() {
            this._disposed = true;
            var c = 0;
            PluginLog.Debug("Disposing icon textures");
            foreach (var texture in this._iconTextures.Values.Where(texture => texture != null)) {
                c++;
                texture?.Dispose();
            }

            PluginLog.Debug($"Disposed {c} icon textures.");
            this._iconTextures.Clear();
            
            GC.SuppressFinalize(this);
        }
        
        private void LoadIconTexture(int iconId, bool hq = false) {
            Task.Run(() => {
                try {
                    var iconTex = this.GetIcon(iconId, hq) ?? this.GetIcon(0, hq)!;
                    
                    var tex = this._pluginInterface.UiBuilder.LoadImageRaw(iconTex.GetRgbaImageData(), iconTex.Header.Width, iconTex.Header.Height, 4);

                    if (tex.ImGuiHandle != IntPtr.Zero) {
                        this._iconTextures[(iconId, hq)] = tex;
                    } else {
                        tex.Dispose();
                    }
                } catch (Exception ex) {
                    PluginLog.Error($"Failed loading texture for icon {iconId} - {ex.Message}");
                    throw;
                }
            });
        }
        
        public TexFile? GetIcon(int iconId, bool hq = false, bool highres = false) => this.GetIcon(Injections.DataManager.Language, iconId, hq, highres);
        
        public TexFile? GetIcon(ClientLanguage iconLanguage, int iconId, bool hq = false, bool highres = false)
        {
            string type;
            switch (iconLanguage)
            {
                case ClientLanguage.Japanese:
                    type = "ja/";
                    break;
                case ClientLanguage.English:
                    type = "en/";
                    break;
                case ClientLanguage.German:
                    type = "de/";
                    break;
                case ClientLanguage.French:
                    type = "fr/";
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(iconLanguage), "Unknown Language: " + Injections.DataManager.Language);
            }
            return this.GetIcon(type, iconId, hq, highres);
        }

        public TexFile? GetIcon(string type, int iconId, bool hq = false, bool highres = false) {
            if (type.Length > 0 && !type.EndsWith("/"))
                type += "/";
            
            var formatStr = $"ui/icon/{{0:D3}}000/{(hq?"hq/":"")}{{1}}{{2:D6}}{(highres?"_hr1":"")}.tex";
            TexFile? file = Injections.DataManager.GetFile<TexFile>(string.Format(formatStr, (iconId / 1000), type, iconId));

            if (file == null && highres) {
                // high-res probably doesn't exist, fallback and try low-res instead
                PluginLog.Debug($"Couldn't get high res icon for {string.Format(formatStr, (iconId / 1000), type, iconId)}");
                return this.GetIcon(type, iconId, hq);
            }
            
            return file != null || type.Length <= 0 ? file : Injections.DataManager.GetFile<TexFile>(string.Format(formatStr, (iconId / 1000), string.Empty, iconId));
        }

        public TextureWrap? GetIconTexture(int iconId, bool hq = false) {
            if (this._disposed) return null;
            if (this._iconTextures.ContainsKey((iconId, hq))) return this._iconTextures[(iconId, hq)];
            this._iconTextures.Add((iconId, hq), null);
            this.LoadIconTexture(iconId, hq);
            return this._iconTextures[(iconId, hq)];
        }
        
        
        private static Image GetImage(TexFile tex)
        {
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
}
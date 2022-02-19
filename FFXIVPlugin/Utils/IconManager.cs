using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Dalamud;
using Dalamud.Logging;
using Dalamud.Plugin;
using Dalamud.Utility;
using FFXIVPlugin.helpers;
using ImGuiScene;
using Lumina.Data.Files;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Action = Lumina.Excel.GeneratedSheets.Action;

namespace FFXIVPlugin.Utils {
    
    // borrowed from https://github.com/Caraxi/RemindMe/blob/master/IconManager.cs
    public class IconManager : IDisposable {

        private readonly DalamudPluginInterface _pluginInterface;
        private bool _disposed;
        private readonly Dictionary<(int, bool), TextureWrap> _iconTextures = new();
        private readonly Dictionary<uint, ushort> _actionCustomIcons = new() {
            
        };

        public IconManager(DalamudPluginInterface pluginInterface) {
            this._pluginInterface = pluginInterface;
        }
        
        public void Dispose() {
            _disposed = true;
            var c = 0;
            PluginLog.Debug("Disposing icon textures");
            foreach (var texture in _iconTextures.Values.Where(texture => texture != null)) {
                c++;
                texture.Dispose();
            }

            PluginLog.Debug($"Disposed {c} icon textures.");
            _iconTextures.Clear();
        }
        
        private void LoadIconTexture(int iconId, bool hq = false) {
            Task.Run(() => {
                try {
                    var iconTex = GetIcon(iconId, hq);
                    
                    var tex = _pluginInterface.UiBuilder.LoadImageRaw(iconTex.GetRgbaImageData(), iconTex.Header.Width, iconTex.Header.Height, 4);

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
        
        public TexFile GetIcon(int iconId, bool hq = false, bool highres = false) => this.GetIcon(Injections.DataManager.Language, iconId, hq, highres);
        
        public TexFile GetIcon(ClientLanguage iconLanguage, int iconId, bool hq = false, bool highres = false)
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
                    throw new ArgumentOutOfRangeException("Language", "Unknown Language: " + Injections.DataManager.Language.ToString());
            }
            return this.GetIcon(type, iconId, hq, highres);
        }

        public TexFile GetIcon(string type, int iconId, bool hq = false, bool highres = false) 
        {
            if (type == null)
                type = string.Empty;
            if (type.Length > 0 && !type.EndsWith("/"))
                type += "/";
            
            var formatStr = $"ui/icon/{{0:D3}}000/{(hq?"hq/":"")}{{1}}{{2:D6}}{(highres?"_hr1":"")}.tex";
            TexFile file = Injections.DataManager.GetFile<TexFile>(string.Format(formatStr, (iconId / 1000), type, iconId));

            if (file == null && highres) {
                // high-res probably doesn't exist, fallback and try low-res instead
                PluginLog.Debug($"Couldn't get high res icon for {string.Format(formatStr, (iconId / 1000), type, iconId)}");
                return GetIcon(type, iconId, hq, false);
            }
            
            return file != null || type.Length <= 0 ? file : Injections.DataManager.GetFile<TexFile>(string.Format(formatStr, (iconId / 1000), string.Empty, iconId));
        }


        public TextureWrap GetActionIcon(Action action) {
            return GetIconTexture(_actionCustomIcons.ContainsKey(action.RowId) ? _actionCustomIcons[action.RowId] : action.Icon);
        }

        public ushort GetActionIconId(Action action) {
            return _actionCustomIcons.ContainsKey(action.RowId) ? _actionCustomIcons[action.RowId] : action.Icon;
        }

        public TextureWrap GetIconTexture(int iconId, bool hq = false) {
            if (this._disposed) return null;
            if (this._iconTextures.ContainsKey((iconId, hq))) return this._iconTextures[(iconId, hq)];
            this._iconTextures.Add((iconId, hq), null);
            LoadIconTexture(iconId, hq);
            return this._iconTextures[(iconId, hq)];
        }
        
        
        private static Image GetImage(TexFile tex)
        {
            return Image.LoadPixelData<Bgra32>(tex.ImageData, tex.Header.Width, tex.Header.Height);
        }

        public string GetIconAsPngString(int iconId, bool hq = false) {
            var icon = GetIcon("", iconId, hq, true);
            var image = GetImage(icon);

            using (MemoryStream stream = new MemoryStream()) {
                image.SaveAsPng(stream);
                byte[] pngBytes = stream.ToArray();
                
                return "data:image/png;base64," + Convert.ToBase64String(pngBytes);
            }
        }
    }
}
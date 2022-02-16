﻿using System;
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
public class IconManager : IDisposable {

        private readonly DalamudPluginInterface pluginInterface;
        private bool disposed;
        private readonly Dictionary<(int, bool), TextureWrap> iconTextures = new();
        private readonly Dictionary<uint, ushort> actionCustomIcons = new() {
            
        };

        public IconManager(DalamudPluginInterface pluginInterface) {
            this.pluginInterface = pluginInterface;
        }
        
        public void Dispose() {
            disposed = true;
            var c = 0;
            PluginLog.Log("Disposing icon textures");
            foreach (var texture in iconTextures.Values.Where(texture => texture != null)) {
                c++;
                texture.Dispose();
            }

            PluginLog.Log($"Disposed {c} icon textures.");
            iconTextures.Clear();
        }
        
        private void LoadIconTexture(int iconId, bool hq = false) {
            Task.Run(() => {
                try {
                    var iconTex = GetIcon(iconId, hq);
                    
                    PluginLog.Log($"{(iconTex == null)}");

                    var tex = pluginInterface.UiBuilder.LoadImageRaw(iconTex.GetRgbaImageData(), iconTex.Header.Width, iconTex.Header.Height, 4);

                    if (tex.ImGuiHandle != IntPtr.Zero) {
                        this.iconTextures[(iconId, hq)] = tex;
                    } else {
                        tex.Dispose();
                    }
                } catch (Exception ex) {
                    PluginLog.LogError($"Failed loading texture for icon {iconId} - {ex.Message}");
                    throw;
                }
            });
        }
        
        public TexFile GetIcon(int iconId, bool hq = false) => this.GetIcon(Injections.DataManager.Language, iconId, hq);

        /// <summary>
        /// Get a <see cref="T:Lumina.Data.Files.TexFile" /> containing the icon with the given ID, of the given language.
        /// </summary>
        /// <param name="iconLanguage">The requested language.</param>
        /// <param name="iconId">The icon ID.</param>
        /// <returns>The <see cref="T:Lumina.Data.Files.TexFile" /> containing the icon.</returns>
        public TexFile GetIcon(ClientLanguage iconLanguage, int iconId, bool hq = false)
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
            return this.GetIcon(type, iconId, hq);
        }
        
        public TexFile GetIcon(string type, int iconId, bool hq = false)
        {
            if (type == null)
                type = string.Empty;
            if (type.Length > 0 && !type.EndsWith("/"))
                type += "/";
            
            var formatStr = $"ui/icon/{{0:D3}}000/{(hq?"hq/":"")}{{1}}{{2:D6}}.tex";
            TexFile file = Injections.DataManager.GetFile<TexFile>(string.Format(formatStr, (object) (iconId / 1000), (object) type, (object) iconId));
            return file != null || type.Length <= 0 ? file : Injections.DataManager.GetFile<TexFile>(string.Format(formatStr, (object) (iconId / 1000), (object) string.Empty, (object) iconId));
        }
        

        public TextureWrap GetActionIcon(Action action) {
            return GetIconTexture(actionCustomIcons.ContainsKey(action.RowId) ? actionCustomIcons[action.RowId] : action.Icon);
        }

        public ushort GetActionIconId(Action action) {
            return actionCustomIcons.ContainsKey(action.RowId) ? actionCustomIcons[action.RowId] : action.Icon;
        }

        public TextureWrap GetIconTexture(int iconId, bool hq = false) {
            if (this.disposed) return null;
            if (this.iconTextures.ContainsKey((iconId, hq))) return this.iconTextures[(iconId, hq)];
            this.iconTextures.Add((iconId, hq), null);
            LoadIconTexture(iconId, hq);
            return this.iconTextures[(iconId, hq)];
        }
        
        
        private static Image GetImage(TexFile tex)
        {
            return Image.LoadPixelData<Bgra32>(tex.ImageData, tex.Header.Width, tex.Header.Height);
        }

        public string GetIconAsPngString(int iconId, bool hq = false) {
            var icon = GetIcon(iconId, hq);
            var image = GetImage(icon);

            byte[] pngBytes = null;
            using (MemoryStream stream = new MemoryStream()) {
                image.SaveAsPng(stream);
                pngBytes = stream.ToArray();
                
                return "data:image/png;base64," + Convert.ToBase64String(pngBytes);
            }
        }
    }
}
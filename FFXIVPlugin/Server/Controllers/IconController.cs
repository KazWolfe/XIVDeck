using System;
using System.Threading.Tasks;
using EmbedIO;
using EmbedIO.Routing;
using EmbedIO.WebApi;
using SixLabors.ImageSharp.Processing;
using XIVDeck.FFXIVPlugin.Game.Managers;
using XIVDeck.FFXIVPlugin.Server.Helpers;
using XIVDeck.FFXIVPlugin.Utils;
using Rectangle = SixLabors.ImageSharp.Rectangle;

namespace XIVDeck.FFXIVPlugin.Server.Controllers;

[ApiController("/icon")]
public class IconController : WebApiController {
    
    [Route(HttpVerbs.Get, "/{iconId}")]
    public async Task GetIcon(int iconId, [QueryField] bool hq = false) {
        this.HttpContext.Response.ContentType = "image/png";
        
        var icon = IconManager.GetIcon("", iconId, hq, true);

        if (icon == null)
            throw HttpException.NotFound($"Icon {iconId} was not found.");

        await using var stream = this.HttpContext.OpenResponseStream();
        await stream.WriteAsync(icon.GetImage().ConvertToPng());
    }

    [Route(HttpVerbs.Get, "/uld/{uldName}")]
    public async Task GetUldTexture(string uldName, [QueryField] int? partId, [QueryField] int? subPartId, [QueryField] bool? highres) {
        this.HttpContext.Response.ContentType = "image/png";
        highres ??= true;

        var uldPath = $"ui/uld/{uldName}";
        var tex = IconManager.GetTexture($"{uldPath}{(highres.Value ? "_hr1" : "")}.tex");

        if (tex == null)
            throw HttpException.NotFound($"Texture for {uldPath} was not found.");

        var image = tex.GetImage();

        if (partId != null && subPartId != null) {
            (int X, int Y, int W, int H) uldRect;
            var scale = highres.Value ? 2 : 1;

            try {
                uldRect = IconManager.GetUldRect(uldName, partId.Value, subPartId.Value);
            } catch (ArgumentException ex) {
                throw HttpException.NotFound($"ULD {uldName} was not found", ex);
            } catch (IndexOutOfRangeException ex) {
                throw HttpException.NotFound($"Part {partId}.{subPartId} was not found in ULD {uldName}", ex);
            }
            
            image.Mutate(t => t.Crop(Rectangle.FromLTRB(
                    scale * (uldRect.X), 
                    scale * (uldRect.Y), 
                    scale * (uldRect.X + uldRect.W),
                    scale * (uldRect.Y + uldRect.H)
                )
            ));
        }

        await using var stream = this.HttpContext.OpenResponseStream();
        await stream.WriteAsync(image.ConvertToPng());
    }
}



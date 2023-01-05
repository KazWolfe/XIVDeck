using System.Threading.Tasks;
using EmbedIO;
using EmbedIO.Routing;
using EmbedIO.WebApi;
using XIVDeck.FFXIVPlugin.Game.Managers;
using XIVDeck.FFXIVPlugin.Server.Helpers;
using XIVDeck.FFXIVPlugin.Utils;

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
}



using System.Threading.Tasks;
using EmbedIO;
using EmbedIO.Routing;
using EmbedIO.WebApi;
using XIVDeck.FFXIVPlugin.Server.Helpers;

namespace XIVDeck.FFXIVPlugin.Server.Controllers;

[ApiController(("/icon"))]
public class IconController : WebApiController {
    
    [Route(HttpVerbs.Get, "/{iconId}")]
    public async Task GetIcon(int iconId, [QueryField] bool hq = false) {
        this.HttpContext.Response.ContentType = "image/png";
        
        var iconData = XIVDeckPlugin.Instance.IconManager.GetIconAsPng(iconId, hq);

        await using var stream = this.HttpContext.OpenResponseStream();
        await stream.WriteAsync(iconData);
    }
}



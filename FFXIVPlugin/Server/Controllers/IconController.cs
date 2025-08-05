using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using EmbedIO;
using EmbedIO.Routing;
using EmbedIO.WebApi;
using XIVDeck.FFXIVPlugin.Base;
using XIVDeck.FFXIVPlugin.Game.Managers;
using XIVDeck.FFXIVPlugin.Server.Helpers;
using XIVDeck.FFXIVPlugin.Utils;

namespace XIVDeck.FFXIVPlugin.Server.Controllers;

[ApiController("/icon")]
public class IconController : WebApiController {
    private static ConcurrentDictionary<int, byte[]> _iconCache = new();

    [Route(HttpVerbs.Get, "/{iconId}")]
    public async Task GetIcon(int iconId) {
        this.HttpContext.Response.ContentType = "image/png";
        await using var stream = this.HttpContext.OpenResponseStream();


        if (_iconCache.TryGetValue(iconId, out var value)) {
            await stream.WriteAsync(value);
            return;
        }

        Injections.PluginLog.Debug("Cache miss for icon {IconId}, fetching from game data.", iconId);

        var icon = IconManager.GetIcon("", iconId, true);

        if (icon == null)
            throw HttpException.NotFound($"Icon {iconId} was not found.");

        var iconData = await icon.GetImage().ConvertToPngAsync();
        _iconCache[iconId] = iconData;
        await stream.WriteAsync(iconData);
    }
}



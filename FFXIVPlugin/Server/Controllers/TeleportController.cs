using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using EmbedIO;
using EmbedIO.Routing;
using EmbedIO.WebApi;
using XIVDeck.FFXIVPlugin.Base;
using XIVDeck.FFXIVPlugin.Game.Managers;
using XIVDeck.FFXIVPlugin.Server.Helpers;
using XIVDeck.FFXIVPlugin.Server.Types;
using XIVDeck.FFXIVPlugin.Utils;

namespace XIVDeck.FFXIVPlugin.Server.Controllers;

[ApiController("/teleport")]
public class TeleportController : WebApiController {

    [Route(HttpVerbs.Get, "/")]
    public List<SerializableAetheryte> GetAetherytes() {
        return Injections.AetheryteList.Select(entry => new SerializableAetheryte(entry)).ToList();
    }

    [Route(HttpVerbs.Post, "/{id}/{subId}/execute")] 
    public void TeleportToAetheryte(uint id, byte subId = 0) {
        TeleportManager.Teleport(id, subId);
    }
}



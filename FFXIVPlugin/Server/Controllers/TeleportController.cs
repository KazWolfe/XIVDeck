using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using EmbedIO;
using EmbedIO.Routing;
using EmbedIO.WebApi;
using Lumina.Excel.GeneratedSheets;
using XIVDeck.FFXIVPlugin.Base;
using XIVDeck.FFXIVPlugin.Exceptions;
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

    [Route(HttpVerbs.Get, "/{id}/{subId}")]
    public SerializableAetheryte GetAetheryte(uint id, byte subId = 0) {
        var aetheryte = TeleportManager.GetAetheryte(id, subId);
        if (aetheryte != null) {
            return new SerializableAetheryte(aetheryte);
        }

        var luminaAetheryte = Injections.DataManager.GetExcelSheet<Aetheryte>()!.GetRow(id);
        if (luminaAetheryte != null) {
            return new SerializableAetheryte(luminaAetheryte, subId);
        }

        throw HttpException.NotFound($"No aetheryte with ID {id} exists.");
    }

    [Route(HttpVerbs.Post, "/{id}/{subId}/execute")] 
    public void TeleportToAetheryte(uint id, byte subId = 0) {
        if (!Injections.ClientState.IsLoggedIn)
            throw new PlayerNotLoggedInException();

        var aetheryte = TeleportManager.GetAetheryte(id, subId);
        if (aetheryte != null) {
            TeleportManager.Teleport(aetheryte);
            return;
        }
        
        var luminaAetheryte = Injections.DataManager.GetExcelSheet<Aetheryte>()!.GetRow(id);
        if (luminaAetheryte == null)
            throw HttpException.NotFound($"No aetheryte with ID {id} exists.");

        if (subId > 0)
            throw new ActionLockedException("The requested housing aetheryte is not available.");

        throw new ActionLockedException($"The requested aetheryte is locked or cannot be found.");
    }
}



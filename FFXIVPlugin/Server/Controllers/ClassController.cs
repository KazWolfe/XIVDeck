using System.Collections.Generic;
using System.Linq;
using Dalamud.Logging;
using EmbedIO;
using EmbedIO.Routing;
using EmbedIO.WebApi;
using Lumina.Excel.GeneratedSheets;
using XIVDeck.FFXIVPlugin.Base;
using XIVDeck.FFXIVPlugin.Game;
using XIVDeck.FFXIVPlugin.Server.Helpers;
using XIVDeck.FFXIVPlugin.Server.Types;
using XIVDeck.FFXIVPlugin.Utils;

namespace XIVDeck.FFXIVPlugin.Server.Controllers;

[ApiController("/classes")]
public class ClassController : WebApiController {
    private readonly GameStateCache _gameStateCache = XIVDeckPlugin.Instance.GameStateCache;

    [Route(HttpVerbs.Get, "/")]
    public List<SerializableGameClass> GetClasses() {
        return SerializableGameClass.GetCache();
    }
    
    [Route(HttpVerbs.Get, "/available")]
    public List<SerializableGameClass> GetAvailableClasses() {
        this._gameStateCache.Refresh();

        var availableClasses = this._gameStateCache.Gearsets!
            .Select(gearset => (int) gearset.ClassJob)
            .ToList();

        return SerializableGameClass.GetCache().Where(gameClass => availableClasses.Contains(gameClass.Id)).ToList();
    }

    [Route(HttpVerbs.Get, "/{id}")]
    public SerializableGameClass GetClass(int id) {
        return SerializableGameClass.GetCache()[id];
    }

    [Route(HttpVerbs.Post, "/{id}/execute")]
    public void SwitchClass(int id) {
        if (id < 1) 
            throw HttpException.BadRequest("Cannot switch to a class with ID less than 1.");
        
        if (!Injections.ClientState.IsLoggedIn)
            throw HttpException.BadRequest("A player is not logged in to the game.");

        this._gameStateCache.Refresh();
        
        foreach (var gearset in this._gameStateCache.Gearsets!) {
            if (gearset.ClassJob != id) continue;
            
            TickScheduler.Schedule(delegate {
                var command = $"/gs change {gearset.Slot}";
                PluginLog.Debug($"Would send command: {command}");
                ChatUtils.SendSanitizedChatMessage(command);
            });
            
            return;
        }
        
        // pretty error handling
        var sheet = Injections.DataManager.Excel.GetSheet<ClassJob>();
        var classJob = sheet!.GetRow((uint) id);

        if (classJob == null) 
            throw HttpException.NotFound($"A class with ID {id} does not exist!");
        
        throw HttpException.BadRequest($"Couldn't switch to {classJob.NameEnglish} because you " +
                                 $"don't have a gearset for this class. Make one and try again.");
    }
}


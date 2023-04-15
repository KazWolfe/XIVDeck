using System.Collections.Generic;
using System.Linq;
using Dalamud.Logging;
using EmbedIO;
using EmbedIO.Routing;
using EmbedIO.WebApi;
using Lumina.Excel.GeneratedSheets;
using XIVDeck.FFXIVPlugin.Base;
using XIVDeck.FFXIVPlugin.Exceptions;
using XIVDeck.FFXIVPlugin.Game;
using XIVDeck.FFXIVPlugin.Game.Chat;
using XIVDeck.FFXIVPlugin.Resources.Localization;
using XIVDeck.FFXIVPlugin.Server.Helpers;
using XIVDeck.FFXIVPlugin.Server.Types;

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
            throw HttpException.BadRequest(UIStrings.ClassController_ClassLessThan1Error);

        if (!Injections.ClientState.IsLoggedIn)
            throw new PlayerNotLoggedInException();
        
        var sheet = Injections.DataManager.Excel.GetSheet<ClassJob>();
        var classJob = sheet!.GetRow((uint) id);

        if (classJob == null)
            throw HttpException.NotFound(string.Format(UIStrings.ClassController_InvalidClassIdError, id));

        GameUtils.ResetAFKTimer();
        this._gameStateCache.Refresh();

        while (true) {
            foreach (var gearset in this._gameStateCache.Gearsets!) {
                if (gearset.ClassJob != id) continue;

                Injections.Framework.RunOnFrameworkThread(delegate {
                    var command = $"/gs change {gearset.Slot}";
                    PluginLog.Debug($"Would send command: {command}");
                    ChatHelper.GetInstance().SendSanitizedChatMessage(command);
                });

                // notify the user on fallback
                if (id != classJob.RowId) {
                    var fallbackClassJob = sheet.GetRow((uint) id)!;

                    PluginLog.Information($"Used fallback {fallbackClassJob.Abbreviation} for requested {classJob.Abbreviation}");
                    ErrorNotifier.ShowError(string.Format(
                        UIStrings.ClassController_FallbackClassUsed, 
                        UIStrings.Culture.TextInfo.ToTitleCase(classJob.Name), 
                        UIStrings.Culture.TextInfo.ToTitleCase(fallbackClassJob.Name)), true);
                }

                return;
            }

            // fallback logic
            var parentId = classJob.ClassJobParent.Row;
            if (parentId == id || parentId == 0) {
                PluginLog.Debug($"Couldn't find a fallback class for {classJob.Abbreviation}");
                break;
            }

            id = (int) parentId;
        }

        throw HttpException.BadRequest(
            string.Format(UIStrings.ClassController_NoGearsetForClassError, 
                UIStrings.Culture.TextInfo.ToTitleCase(classJob.Name)));
    }
}
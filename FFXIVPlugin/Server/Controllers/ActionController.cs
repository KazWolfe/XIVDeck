using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using EmbedIO;
using EmbedIO.Routing;
using EmbedIO.WebApi;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using XIVDeck.FFXIVPlugin.ActionExecutor;
using XIVDeck.FFXIVPlugin.Exceptions;
using XIVDeck.FFXIVPlugin.Server.Helpers;

namespace XIVDeck.FFXIVPlugin.Server.Controllers; 

[ApiController("/action")]
public class ActionController : WebApiController {
    [Route(HttpVerbs.Get, "/")]
    public Dictionary<HotbarSlotType, List<ExecutableAction>> GetActions() {
        XIVDeckPlugin.Instance.GameStateCache.Refresh();
        
        Dictionary<HotbarSlotType, List<ExecutableAction>> actions = new();

        foreach (var (type, strategy) in ActionDispatcher.GetStrategies()) {
            var allowedItems = strategy.GetAllowedItems();
            if (allowedItems == null || allowedItems.Count == 0) continue;
            
            actions[type] = allowedItems;
        }

        return actions;
    }

    [Route(HttpVerbs.Get, "/{type}")]
    public List<ExecutableAction> GetActionsByType(string type) {
        if (!Enum.TryParse<HotbarSlotType>(type, out var slotType)) {
            throw HttpException.NotFound($"No registered action of type {type} was found.");
        }

        var actions = this.GetActions();
        return actions[slotType];
    }

    [Route(HttpVerbs.Get, "/{type}/{id}")]
    public ExecutableAction GetAction(string type, int id) {
        if (!Enum.TryParse<HotbarSlotType>(type, out var slotType)) {
            throw HttpException.NotFound($"No registered action of type {type} was found.");
        }

        var action = ActionDispatcher.GetStrategyForSlotType(slotType).GetExecutableActionById((uint) id);
        
        if (action == null) {
            throw HttpException.NotFound($"No action with {id} exists for type {type}");
        }

        return action;
    }

    [Route(HttpVerbs.Post, "/{type}/{id}/execute")]
    public void ExecuteAction(string type, int id, [QueryData] NameValueCollection options) {
        if (!Enum.TryParse<HotbarSlotType>(type, out var slotType)) {
            throw HttpException.NotFound($"No registered action of type {type} was found.");
        }

        try {
            ActionDispatcher.Execute(slotType, id);
        } catch (PlayerNotLoggedInException ex) {
            throw HttpException.Unauthorized(ex.Message, ex);
        } catch (ActionLockedException ex) {
            throw HttpException.Forbidden(ex.Message, ex);
        } catch (ActionNotFoundException ex) {
            throw HttpException.NotFound(ex.Message, ex);
        } catch (IllegalGameStateException ex) {
            throw HttpException.BadRequest(ex.Message, ex);
        }
    }
}
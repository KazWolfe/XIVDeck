using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using EmbedIO;
using EmbedIO.Routing;
using EmbedIO.WebApi;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using Newtonsoft.Json;
using XIVDeck.FFXIVPlugin.ActionExecutor;
using XIVDeck.FFXIVPlugin.Base;
using XIVDeck.FFXIVPlugin.Exceptions;
using XIVDeck.FFXIVPlugin.Game;
using XIVDeck.FFXIVPlugin.Resources.Localization;
using XIVDeck.FFXIVPlugin.Server.Helpers;

namespace XIVDeck.FFXIVPlugin.Server.Controllers;

[ApiController("/action")]
public class ActionController : WebApiController {
    [Route(HttpVerbs.Get, "/")]
    public Dictionary<HotbarSlotType, List<ExecutableAction>> GetActions() {
        Dictionary<HotbarSlotType, List<ExecutableAction>> actions = new();

        foreach (var (type, strategy) in XIVDeckPlugin.Instance.ActionDispatcher.GetStrategies()) {
            var allowedItems = strategy.GetAllowedItems();
            if (allowedItems == null || allowedItems.Count == 0) continue;

            actions[type] = allowedItems;
        }

        return actions;
    }

    [Route(HttpVerbs.Get, "/{type}")]
    public List<ExecutableAction> GetActionsByType(string type) {
        if (!Enum.TryParse<HotbarSlotType>(type, out var slotType)) {
            throw HttpException.NotFound(string.Format(UIStrings.ActionController_UnknownActionTypeError, type));
        }

        var strategy = XIVDeckPlugin.Instance.ActionDispatcher.GetStrategyForType(slotType);
        return strategy.GetAllowedItems() ?? new List<ExecutableAction>();
    }

    [Route(HttpVerbs.Get, "/{type}/{id}")]
    public ExecutableAction GetAction(string type, int id) {
        if (!Enum.TryParse<HotbarSlotType>(type, out var slotType)) {
            throw HttpException.NotFound(string.Format(UIStrings.ActionController_UnknownActionTypeError, type));
        }

        var action = XIVDeckPlugin.Instance.ActionDispatcher.GetStrategyForType(slotType).GetExecutableActionById((uint) id);

        if (action == null) {
            throw new ActionNotFoundException(slotType, (uint) id);
        }

        return action;
    }

    [Route(HttpVerbs.Post, "/{type}/{id}/execute")]
    public async Task ExecuteAction(string type, int id) {
        if (!Enum.TryParse<HotbarSlotType>(type, out var slotType))
            throw HttpException.NotFound(string.Format(UIStrings.ActionController_UnknownActionTypeError, type));

        if (!Injections.ClientState.IsLoggedIn)
            throw new PlayerNotLoggedInException();
        
        var strategy = XIVDeckPlugin.Instance.ActionDispatcher.GetStrategyForType(slotType);
        var payloadType = strategy.GetPayloadType();

        ActionPayload? payload = null;
        if (payloadType != null) {
            var requestBody = await this.HttpContext.GetRequestBodyAsStringAsync();
            payload = JsonConvert.DeserializeObject(requestBody, payloadType) as ActionPayload;
            
            Injections.PluginLog.Debug($"Body: {requestBody}\nPayload: {payload}");
        }

        GameUtils.ResetAFKTimer();
        strategy.Execute((uint) id, payload);
    }
}
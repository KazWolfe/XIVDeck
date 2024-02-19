using System;
using System.Collections.Generic;
using System.Linq;
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
    /// <summary>
    /// A lookup table of aliases for certain action types. Used for cases where an action type may have changed
    /// names as a result of CS updates or similar.
    /// </summary>
    private static readonly Dictionary<string, HotbarSlotType> ActionTypeAliases = new() {
        { "Minion", HotbarSlotType.Companion },
        { "FashionAccessory", HotbarSlotType.Ornament }
    };

    private static readonly Dictionary<HotbarSlotType, string> SlotTypeNames = ActionTypeAliases.Reverse()
        .ToDictionary(v => v.Value, k => k.Key);
    
    [Route(HttpVerbs.Get, "/")]
    public Dictionary<string, List<ExecutableAction>> GetActions() {
        Dictionary<string, List<ExecutableAction>> actions = new();

        foreach (var (type, strategy) in XIVDeckPlugin.Instance.ActionDispatcher.GetStrategies()) {
            var allowedItems = strategy.GetAllowedItems();
            if (allowedItems == null || allowedItems.Count == 0) continue;

            var typeName = SlotTypeNames.GetValueOrDefault(type, type.ToString());

            actions[typeName] = allowedItems;
        }

        return actions;
    }

    [Route(HttpVerbs.Get, "/{type}")]
    public List<ExecutableAction> GetActionsByType(string type) {
        if (!TryGetSlotTypeByName(type, out var slotType)) {
            throw HttpException.NotFound(string.Format(UIStrings.ActionController_UnknownActionTypeError, type));
        }

        var strategy = XIVDeckPlugin.Instance.ActionDispatcher.GetStrategyForType(slotType);
        return strategy.GetAllowedItems() ?? new List<ExecutableAction>();
    }

    [Route(HttpVerbs.Get, "/{type}/{id}")]
    public ExecutableAction GetAction(string type, int id) {
        if (!TryGetSlotTypeByName(type, out var slotType)) {
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
        if (!TryGetSlotTypeByName(type, out var slotType))
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

    private static bool TryGetSlotTypeByName(string typeName, out HotbarSlotType slotType) {
        return ActionTypeAliases.TryGetValue(typeName, out slotType) || Enum.TryParse(typeName, out slotType);
    }
}
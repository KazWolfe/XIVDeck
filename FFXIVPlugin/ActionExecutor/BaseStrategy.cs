using System;
using System.Collections.Generic;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace XIVDeck.FFXIVPlugin.ActionExecutor {
    [Serializable]
    public class ExecutableAction {
        [JsonProperty("name")] public string? ActionName; // optional, will realistically only ever be sent
        [JsonProperty("id")] public int ActionId;
        [JsonProperty("iconId")] public int IconId;

        [JsonProperty("category")] public string? Category; // optional, send-only. used for grouping where available
        
        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty("type")] public HotbarSlotType HotbarSlotType;
    }

    public interface IActionStrategy {
        /**
         * Execute an event with the given Action ID, depending on the strategy for this action type.
         */
        public void Execute(uint actionId, dynamic? options = null);

        /**
         * Get the Icon ID used for a specific action type
         */
        public int GetIconId(uint actionId);

        /**
         * Get a specific action regardless of unlock state by ID
         */
        public ExecutableAction? GetExecutableActionById(uint actionId);
        
        /**
         * Get a dynamic list of items allowed by this strategy.
         * 
         */
        public List<ExecutableAction>? GetAllowedItems();
    }
}
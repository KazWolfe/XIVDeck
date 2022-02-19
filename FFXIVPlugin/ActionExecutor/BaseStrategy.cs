using System;
using System.Collections.Generic;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using FFXIVClientStructs.FFXIV.Component.Excel;
using FFXIVPlugin.helpers;
using Lumina.Excel;
using Lumina.Excel.GeneratedSheets;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace FFXIVPlugin.ActionExecutor {
    public class ExecutableAction {
        [JsonProperty("name")] public string ActionName; // optional, will realistically only ever be sent
        [JsonProperty("id")] public int ActionId;
        
        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty("type")] public HotbarSlotType HotbarSlotType;

        [NonSerialized] public ExcelSheet LuminaActionType;
    }
    
    public interface IBaseStrategy {

        /**
         * Execute an event with the given Action ID, depending on the strategy for this action type.
         */
        public abstract void Execute(uint actionId, dynamic options = null);

        /**
         * Get the Icon ID used for a specific action type
         */
        public abstract int GetIconId(uint actionId);

        /**
         * Get a dynamic list of items allowed by this strategy.
         * 
         */
        public abstract List<ExecutableAction> GetAllowedItems();
        
    }
}
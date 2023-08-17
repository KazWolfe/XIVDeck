using System;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace XIVDeck.FFXIVPlugin.ActionExecutor; 

[Serializable]
public class ExecutableAction {
    [JsonProperty("name")] public string? ActionName; // optional, will realistically only ever be sent
    [JsonProperty("id")] public int ActionId;
    [JsonProperty("iconId")] public int IconId;
    [JsonProperty("sortOrder")] public int? SortOrder;

    [JsonProperty("category")] public string? Category; // optional, send-only. used for grouping where available
        
    [JsonConverter(typeof(StringEnumConverter))]
    [JsonProperty("type")] public HotbarSlotType HotbarSlotType;

    [JsonProperty("cooldownGroup")] 
    public int? CooldownGroup;
}
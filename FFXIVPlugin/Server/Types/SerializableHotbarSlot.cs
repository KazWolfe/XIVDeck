using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace XIVDeck.FFXIVPlugin.Server.Types {
    public class SerializableHotbarSlot {
        [JsonProperty("hotbarId")] public int HotbarId;
        [JsonProperty("slotId")] public int SlotId;

        [JsonProperty("type")] [JsonConverter(typeof(StringEnumConverter))]
        public HotbarSlotType SlotType;

        [JsonProperty("commandId")] public int CommandId;
        
        [JsonProperty("iconId")] public int IconId;

        [JsonProperty("iconData")] public string IconData = default!;
    }
}

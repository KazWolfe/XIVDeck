using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using Newtonsoft.Json;

namespace XIVDeck.FFXIVPlugin.Server.Types {
    public class SerializableHotbarSlot {
        [JsonProperty("hotbarId")] public int HotbarId;
        [JsonProperty("slotId")] public int SlotId;

        [JsonProperty("type")] public HotbarSlotType SlotType;
        [JsonProperty("iconId")] public int IconId;

        [JsonProperty("iconData")] public string IconData = default!;
    }
}

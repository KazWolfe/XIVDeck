using static FFXIVClientStructs.FFXIV.Client.UI.Misc.RaptureHotbarModule;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace XIVDeck.FFXIVPlugin.Server.Types;

public class SerializableHotbarSlot {
    [JsonProperty("hotbarId")] public int HotbarId;
    [JsonProperty("slotId")] public int SlotId;

    [JsonProperty("type")] [JsonConverter(typeof(StringEnumConverter))]
    public HotbarSlotType SlotType;

    [JsonProperty("commandId")] public int CommandId;

    [JsonProperty("iconId")] public int IconId;
}

public record MicroHotbarSlot(int HotbarId, int SlotId) {
    [JsonProperty("hotbarId")] public int HotbarId = HotbarId;
    [JsonProperty("slotId")] public int SlotId = SlotId;
}

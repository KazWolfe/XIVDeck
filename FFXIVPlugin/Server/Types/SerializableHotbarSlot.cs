﻿using static FFXIVClientStructs.FFXIV.Client.UI.Misc.RaptureHotbarModule;
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

    // Legacy decision to keep this as its own thing. Really, the sdPlugin should be using the normal icon fetch
    // methods, but maintaining this is fine for now.
    [JsonProperty("iconData")] public string IconData = default!;
}

public record MicroHotbarSlot(int HotbarId, int SlotId) {
    [JsonProperty("hotbarId")] public int HotbarId = HotbarId;
    [JsonProperty("slotId")] public int SlotId = SlotId;
}

using Dalamud.Game.ClientState.Aetherytes;
using Lumina.Excel.GeneratedSheets;
using Newtonsoft.Json;
using XIVDeck.FFXIVPlugin.Game;
using XIVDeck.FFXIVPlugin.Game.Managers;

namespace XIVDeck.FFXIVPlugin.Server.Types;

public class SerializableAetheryte {
    [JsonProperty("id")] public uint? Id { get; set; }
    [JsonProperty("subId")] public byte? SubId { get; set; }

    [JsonProperty("name")] public string? Name { get; set; }

    [JsonProperty("region")] public string? Region { get; set; }
    [JsonProperty("territory")] public string? Territory { get; set; }

    [JsonProperty("isHousing")] public bool? IsHousing { get; set; }

    public SerializableAetheryte(AetheryteEntry aetheryteEntry) {
        this.Id = aetheryteEntry.AetheryteId;
        this.SubId = aetheryteEntry.SubIndex;

        this.Name = TeleportManager.GetAetheryteName(aetheryteEntry);

        this.Region = aetheryteEntry.AetheryteData.GameData?.RegionName() ?? "???";
        this.Territory = aetheryteEntry.AetheryteData.GameData?.TerritoryName() ?? "???";

        this.IsHousing = TeleportManager.IsHousingAetheryte(aetheryteEntry);
    }

    public SerializableAetheryte(Aetheryte luminaEntry, byte subId = 0) {
        this.Id = luminaEntry.RowId;
        this.SubId = subId;

        this.Name = TeleportManager.GetAetheryteName(luminaEntry, subId);
        
        this.Region = luminaEntry.RegionName() ?? "???";
        this.Territory = luminaEntry.TerritoryName() ?? "???";

        this.IsHousing = TeleportManager.IsHousingAetheryte(luminaEntry, subId);
    }
}
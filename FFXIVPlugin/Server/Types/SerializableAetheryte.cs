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

    [JsonProperty("worldArea")] public string? WorldArea { get; set; }
    [JsonProperty("territory")] public string? Territory { get; set; }

    [JsonProperty("isHousing")] public bool? IsHousing { get; set; }

    public SerializableAetheryte(AetheryteEntry aetheryteEntry) {
        this.Id = aetheryteEntry.AetheryteId;
        this.SubId = aetheryteEntry.SubIndex;

        this.Name = TeleportManager.GetAetheryteName(aetheryteEntry);

        this.WorldArea = GetWorldArea(aetheryteEntry.AetheryteData.GameData) ?? "???";
        this.Territory = GetRegionTerritory(aetheryteEntry.AetheryteData.GameData) ?? "???";

        this.IsHousing = TeleportManager.IsHousingAetheryte(aetheryteEntry);
    }

    public SerializableAetheryte(Aetheryte luminaEntry, byte subId = 0) {
        this.Id = luminaEntry.RowId;
        this.SubId = subId;

        this.Name = TeleportManager.GetAetheryteName(luminaEntry, subId);

        this.WorldArea = GetWorldArea(luminaEntry) ?? "???";
        this.Territory = GetRegionTerritory(luminaEntry) ?? "???";

        this.IsHousing = TeleportManager.IsHousingAetheryte(luminaEntry, subId);
    }

    private static string? GetWorldArea(Aetheryte? aetheryte) {
        if (aetheryte == null) return null;

        // ughhhhh... this is mapped in a very bad way in-game as well, as far as i can tell.
        return (aetheryte.Unknown23 / 20) switch {
            3 or 4 => AddonTextLoc.GetStringFromRowNumber(8486, "Ishgard and Surrounding Areas"),
            6 => AddonTextLoc.GetStringFromRowNumber(8489, "The Far East"),
            11 => AddonTextLoc.GetStringFromRowNumber(8484, "Others"),
            _ => aetheryte.Territory.Value?.PlaceNameRegion.Value?.Name.RawString,
        };
    }

    private static string? GetRegionTerritory(Aetheryte? aetheryte) {
        if (aetheryte == null) return null;

        var territory = aetheryte.Territory.Value;
        var territoryName = territory?.PlaceName.Value?.Name.RawString;
        var region = territory?.PlaceNameRegion.Value?.Name.RawString;

        return GetWorldArea(aetheryte) != region && region != territoryName
            ? $"{region} - {territoryName}"
            : territoryName;
    }
}
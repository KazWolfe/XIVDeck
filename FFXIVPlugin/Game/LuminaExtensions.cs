using Lumina.Excel.GeneratedSheets;

namespace XIVDeck.FFXIVPlugin.Game; 

public static class LuminaExtensions {
    public static bool IsUnlocked(this Emote emote) => GameStateCache.IsEmoteUnlocked(emote);
    public static bool IsUnlocked(this Companion minion) => GameStateCache.IsMinionUnlocked(minion.RowId);
    public static bool IsUnlocked(this Mount mount) => GameStateCache.IsMountUnlocked(mount.RowId);
    public static bool IsUnlocked(this McGuffin mcguffin) => GameStateCache.IsMcGuffinUnlocked(mcguffin.RowId);
    public static bool IsUnlocked(this Ornament ornament) => GameStateCache.IsOrnamentUnlocked(ornament.RowId);

    public static string TerritoryName(this Aetheryte aetheryte) {
        return aetheryte.Territory.Value?.PlaceName.Value?.Name ?? "???";
    }

    public static string RegionName(this Aetheryte aetheryte) {
        return aetheryte.Territory.Value?.PlaceNameRegion.Value?.Name ?? "???";
    }
}
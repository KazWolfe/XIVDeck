using Lumina.Excel.GeneratedSheets;
using XIVDeck.FFXIVPlugin.Game.Sheets;

namespace XIVDeck.FFXIVPlugin.Game; 

public static class LuminaExtensions {
    public static bool IsUnlocked(this Emote emote) => GameStateCache.IsUnlockLinkUnlockedOrQuestCompleted(emote.UnlockLink);
    public static bool IsUnlocked(this Companion minion) => GameStateCache.IsMinionUnlocked(minion.RowId);
    public static bool IsUnlocked(this Mount mount) => GameStateCache.IsMountUnlocked(mount.RowId);
    public static bool IsUnlocked(this McGuffin mcguffin) => GameStateCache.IsMcGuffinUnlocked(mcguffin.RowId);
    public static bool IsUnlocked(this Ornament ornament) => GameStateCache.IsOrnamentUnlocked(ornament.RowId);
}
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using Lumina.Excel.GeneratedSheets;
using XIVDeck.FFXIVPlugin.Base;

namespace XIVDeck.FFXIVPlugin.Game;

public static unsafe class UnlockState {
    internal static bool IsUnlocked(this Emote emote) {
        // Work around showing emotes if nobody is logged in.
        if (!Injections.ClientState.IsLoggedIn) return false;

        // WARNING: This is a reimplementation of UIState#IsEmoteUnlocked, but designed to hopefully be a bit faster and
        // more reliable. As a result, this is not exactly faithful to how the game does it, but the logic is the same.
        // Particularly:
        // 1. IsEmoteUnlocked will check Emote#EmoteCategory, but we're using Emote#Order as it's more in line with how
        //    the emote UI works.
        // 2. IsEmoteUnlocked uses its own (inlined) checks rather than IULUOQC. However, this inlined version is (for
        //    now) functionally identical to IULUOQC with the default arguments.
        // Both of these decisions *should* be safe, but are being recorded here for posterity for when Square decides
        // to blow all this up.

        if (emote.Order == 0) return false;

        // HACK - We need to handle GC emotes as a special case
        switch (emote.RowId) {
            case 55 when PlayerState.Instance()->GrandCompany != 1: // Maelstrom
            case 56 when PlayerState.Instance()->GrandCompany != 2: // Twin Adders
            case 57 when PlayerState.Instance()->GrandCompany != 3: // Immortal Flames
                return false;
        }

        return emote.UnlockLink == 0 || UIState.Instance()->IsUnlockLinkUnlockedOrQuestCompleted(emote.UnlockLink);
    }

    internal static bool IsUnlocked(this Mount mount) {
        return PlayerState.Instance()->IsMountUnlocked(mount.RowId);
    }
    
    internal static bool IsUnlocked(this Companion minion) {
        return UIState.Instance()->IsCompanionUnlocked(minion.RowId);
    }

    internal static bool IsUnlocked(this Ornament ornament) {
        return PlayerState.Instance()->IsOrnamentUnlocked(ornament.RowId);
    }

    internal static bool IsUnlocked(this McGuffin mcGuffin) {
        return PlayerState.Instance()->IsMcGuffinUnlocked(mcGuffin.RowId);
    }

    internal static bool IsUnlocked(this MainCommand mainCommand) {
        return Framework.Instance()->GetUiModule()->IsMainCommandUnlocked(mainCommand.RowId);
    }
}
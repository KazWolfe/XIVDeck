using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using Lumina.Excel.Sheets;
using XIVDeck.FFXIVPlugin.Base;

namespace XIVDeck.FFXIVPlugin.Game;

public static unsafe class UnlockState {

    /// <summary>
    /// Tests if a specific emote is unlocked. A reimplementation of the in-game method, without the unnecessary EXD
    /// call, as Lumina already has everything we need. Also handles GC and non-executable emote edge cases.
    /// </summary>
    /// <param name="emote">The emote to test against.</param>
    /// <returns>Returns true if the emote is unlocked and valid, false otherwise.</returns>
    internal static bool IsUnlocked(this Emote emote) {
        // Work around showing emotes if nobody is logged in.
        if (!Injections.ClientState.IsLoggedIn) return false;

        if (emote.EmoteCategory.RowId == 0 || emote.Order == 0) return false;

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
        return Framework.Instance()->GetUIModule()->IsMainCommandUnlocked(mainCommand.RowId);
    }

    internal static bool IsUnlocked(this Glasses glasses) {
        return PlayerState.Instance()->IsGlassesUnlocked((ushort)glasses.RowId);
    }
}

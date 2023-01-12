using System.Collections.Generic;
using System.Linq;
using Dalamud.Memory;
using Dalamud.Utility.Signatures;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using Lumina.Excel.GeneratedSheets;
using XIVDeck.FFXIVPlugin.Base;

namespace XIVDeck.FFXIVPlugin.Game;

// borrowed from https://github.com/goaaats/Dalamud.FindAnything/blob/master/Dalamud.FindAnything/GameStateCache.cs
// ... and then promptly adapted and destroyed. I am sorry, goat.
internal unsafe class GameStateCache {
    internal static bool IsEmoteUnlocked(Emote? emote) {
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
        
        if (emote == null || emote.Order == 0) return false;

        // HACK - We need to handle GC emotes as a special case
        switch (emote.RowId) {
            case 55 when PlayerState.Instance()->GrandCompany != 1: // Maelstrom
            case 56 when PlayerState.Instance()->GrandCompany != 2: // Twin Adders
            case 57 when PlayerState.Instance()->GrandCompany != 3: // Immortal Flames
                return false;
        }
        
        return emote.UnlockLink == 0 || UIState.Instance()->IsUnlockLinkUnlockedOrQuestCompleted(emote.UnlockLink);
    }

    internal static bool IsMountUnlocked(uint mountId) {
        return PlayerState.Instance()->IsMountUnlocked(mountId);
    }

    internal static bool IsMinionUnlocked(uint minionId) {
        return UIState.Instance()->IsCompanionUnlocked(minionId);
    }

    internal static bool IsOrnamentUnlocked(uint ornamentId) {
        return PlayerState.Instance()->IsOrnamentUnlocked(ornamentId);
    }

    internal static bool IsMcGuffinUnlocked(uint mcguffinId) {
        return PlayerState.Instance()->IsMcGuffinUnlocked(mcguffinId);
    }

    internal GameStateCache() {
        SignatureHelper.Initialise(this);
        this.Refresh();
    }

    internal struct Gearset {
        public int Slot { get; init; }
        public string Name { get; init; }
        public uint ClassJob { get; init; }
    }

    internal IReadOnlyList<Emote>? UnlockedEmotes { get; private set; }
    internal IReadOnlyList<Mount>? UnlockedMounts { get; private set; }
    internal IReadOnlyList<Companion>? UnlockedMinions { get; private set; }
    internal IReadOnlyList<Ornament>? UnlockedOrnaments { get; private set; }
    internal IReadOnlyList<Gearset>? Gearsets { get; private set; }

    internal void Refresh() {
        this.UnlockedEmotes = Injections.DataManager.GetExcelSheet<Emote>()!
            .Where(x => x.IsUnlocked()).ToList();

        this.UnlockedMounts = Injections.DataManager.GetExcelSheet<Mount>()!
            .Where(x => x.IsUnlocked()).ToList();

        this.UnlockedMinions = Injections.DataManager.GetExcelSheet<Companion>()!
            .Where(x => x.IsUnlocked()).ToList();

        this.UnlockedOrnaments = Injections.DataManager.GetExcelSheet<Ornament>()!
            .Where(x => x.IsUnlocked()).ToList();

        var gearsets = new List<Gearset>();
        for (var i = 0; i < 100; i++) {
            var gs = RaptureGearsetModule.Instance()->Gearset[i];

            if (gs == null || !gs->Flags.HasFlag(RaptureGearsetModule.GearsetFlag.Exists))
                continue;

            var name = MemoryHelper.ReadString(new nint(gs->Name), 47);

            gearsets.Add(new Gearset {
                Slot = i + 1,
                ClassJob = gs->ClassJob,
                Name = name
            });
        }

        this.Gearsets = gearsets;
    }
}
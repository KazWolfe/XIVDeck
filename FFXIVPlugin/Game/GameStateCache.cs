#nullable enable
using System;
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
    private static bool IsUnlockLinkUnlockedOrQuestCompleted(uint unlockLinkOrQuestId) {
        return UIState.Instance()->IsUnlockLinkUnlockedOrQuestCompleted(unlockLinkOrQuestId, 1);
    }
    
    internal static bool IsEmoteUnlocked(Emote? emote) {
        // Work around showing emotes if nobody is logged in.
        if (!Injections.ClientState.IsLoggedIn) return false;
        
        if (emote == null || emote.Order == 0) return false;

        return emote.UnlockLink == 0 || IsUnlockLinkUnlockedOrQuestCompleted(emote.UnlockLink);
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

            var name = MemoryHelper.ReadString(new IntPtr(gs->Name), 47);

            gearsets.Add(new Gearset {
                Slot = i + 1,
                ClassJob = gs->ClassJob,
                Name = name
            });
        }

        this.Gearsets = gearsets;
    }
}
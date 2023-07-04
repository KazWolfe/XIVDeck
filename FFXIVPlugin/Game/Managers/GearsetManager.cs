using System.Collections.Generic;
using Dalamud.Memory;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using XIVDeck.FFXIVPlugin.Game.Data;

namespace XIVDeck.FFXIVPlugin.Game.Managers; 

public static unsafe class GearsetManager {

    public static List<Gearset> GetGearsets() {
        var permittedGearsets = InventoryManager.Instance()->GetPermittedGearsetCount(); // always 100 but eh.
        var gearsets = new List<Gearset>();

        for (var i = 0; i < permittedGearsets; i++) {
            var gearset = GetGearset(i);
            if (gearset == null) continue;

            gearsets.Add(gearset);
        }

        return gearsets;
    }

    public static Gearset? GetGearset(int index) {
        var gs = RaptureGearsetModule.Instance()->Gearset[index];

        if (gs == null || !gs->Flags.HasFlag(RaptureGearsetModule.GearsetFlag.Exists))
            return null;

        return new Gearset {
            Slot = gs->ID + 1,
            Name = MemoryHelper.ReadString(new nint(gs->Name), 47),
            ClassJob = gs->ClassJob
        };
    }
}
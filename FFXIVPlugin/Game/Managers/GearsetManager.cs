using System.Collections.Generic;
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

    /// <summary>
    /// Gets a gearset by its loaded index (passing to GetGearset). If a negative ID is passed, return the current
    /// gearset.
    /// </summary>
    /// <param name="index">The ID of the gearset to retrieve.</param>
    /// <returns>Returns a gearset if one is found.</returns>
    public static Gearset? GetGearset(int index) {
        var gearsetModule = RaptureGearsetModule.Instance();

        if (index < 0)
            index = gearsetModule->CurrentGearsetIndex;

        if (!gearsetModule->IsValidGearset(index))
            return null;

        var gs = gearsetModule->GetGearset(index);

        // technically not needed, but let's be strict anyways.
        if (gs == null || !gs->Flags.HasFlag(RaptureGearsetModule.GearsetFlag.Exists))
            return null;

        return new Gearset {
            Slot = gs->Id + 1,
            Name = gs->NameString,
            ClassJob = gs->ClassJob
        };
    }
}

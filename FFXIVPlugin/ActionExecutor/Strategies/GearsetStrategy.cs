using System;
using System.Collections.Generic;
using System.Linq;
using static FFXIVClientStructs.FFXIV.Client.UI.Misc.RaptureHotbarModule;
using XIVDeck.FFXIVPlugin.ActionExecutor.Payloads;
using XIVDeck.FFXIVPlugin.Base;
using XIVDeck.FFXIVPlugin.Game.Chat;
using XIVDeck.FFXIVPlugin.Game.Data;
using XIVDeck.FFXIVPlugin.Game.Managers;
using XIVDeck.FFXIVPlugin.Resources.Localization;


namespace XIVDeck.FFXIVPlugin.ActionExecutor.Strategies;

[ActionStrategy(HotbarSlotType.GearSet)]
public class GearsetStrategy : IActionStrategy {
    private static ExecutableAction GetExecutableAction(Gearset gearset) {
        return new ExecutableAction {
            ActionId = gearset.Slot,
            ActionName = $"{gearset.Slot}: {gearset.Name}",
            IconId = 062800 + (int)gearset.ClassJob,
            HotbarSlotType = HotbarSlotType.GearSet
        };
    }

    public ExecutableAction? GetExecutableActionById(uint slotId) {
        var gearset = GearsetManager.GetGearset((int)slotId - 1);

        return gearset == null ? null : GetExecutableAction(gearset);
    }

    public List<ExecutableAction> GetAllowedItems() {
        return GearsetManager.GetGearsets()
            .Select(GetExecutableAction)
            .ToList();
    }

    public void Execute(uint actionSlot, ActionPayload? payload) {
        var gearset = GearsetManager.GetGearset((int)actionSlot - 1);

        if (gearset == null)
            throw new ArgumentException(string.Format(UIStrings.GearsetStrategy_GearsetNotFoundError, actionSlot));

        var command = $"/gearset change {gearset.Slot}";

        switch (payload) {
            case GearsetPayload { GlamourPlateId: >= 1 and <= 20 } p:
                command += $" {p.GlamourPlateId}";
                break;
            case GearsetPayload { GlamourPlateId: not null }:
                throw new ArgumentException("Glamour Plate ID must be between 1 and 20.");
        }

        Injections.PluginLog.Debug($"Executing command: {command}");
        Injections.Framework.RunOnFrameworkThread(() => { ChatHelper.SendSanitizedChatMessage(command); });
    }

    public int GetIconId(uint slot) {
        var gearset = GearsetManager.GetGearset((int)slot - 1);

        if (gearset == null) return 0;
        return 062800 + (int)gearset.ClassJob;
    }

    public Type GetPayloadType() => typeof(GearsetPayload);
}

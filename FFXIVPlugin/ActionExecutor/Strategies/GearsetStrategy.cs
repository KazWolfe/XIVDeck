using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dalamud.Plugin.Services;
using Serilog;
using XIVDeck.FFXIVPlugin.ActionExecutor.Payloads;
using static FFXIVClientStructs.FFXIV.Client.UI.Misc.RaptureHotbarModule;
using XIVDeck.FFXIVPlugin.Exceptions;
using XIVDeck.FFXIVPlugin.Game;
using XIVDeck.FFXIVPlugin.Game.Chat;
using XIVDeck.FFXIVPlugin.Game.Managers;
using XIVDeck.FFXIVPlugin.Game.Types;
using XIVDeck.FFXIVPlugin.Resources.Localization;
using ActionAppearance = XIVDeck.FFXIVPlugin.Contract.ActionAppearance;
using XIVDeck.FFXIVPlugin.Contract;
using GearsetPayload = XIVDeck.FFXIVPlugin.ActionExecutor.Payloads.GearsetPayload;

namespace XIVDeck.FFXIVPlugin.ActionExecutor.Strategies;

[ActionStrategy(HotbarSlotType.GearSet)]
public class GearsetStrategy(ILogger pluginLog, IFramework framework, ActionAppearanceResolver appearanceResolver)
    : IActionStrategy {
    private const int MaxGlamourPlateId = 20;

    private static ActionEntry GetExecutableAction(Gearset gearset) {
        return new ActionEntry {
            Id = gearset.Slot,
            Name = $"{gearset.Slot}: {gearset.Name}",
            Type = HotbarSlotType.GearSet
        };
    }

    /// <summary>
    /// For user sanity reasons, we handle gearset slots as a 1-indexed field, similar to how SE displays it in their
    /// UI even at the wire level.
    /// </summary>
    private static Gearset? GetGearsetBySlot(uint slot) {
        return slot == 0 ? null : GearsetManager.GetGearset((int)slot - 1);
    }

    public ActionEntry? GetActionEntryById(uint slotId) {
        var gearset = GetGearsetBySlot(slotId);

        return gearset == null ? null : GetExecutableAction(gearset);
    }

    public List<ActionEntry> GetSelectableActions() {
        return [.. GearsetManager.GetGearsets().Select(GetExecutableAction)];
    }

    public async Task Execute(uint actionSlot, ActionPayload? payload) {
        var gearset = GetGearsetBySlot(actionSlot);

        if (gearset == null)
            throw new ActionNotFoundException(string.Format(UIStrings.GearsetStrategy_GearsetNotFoundError, actionSlot));

        var command = $"/gearset change {gearset.Slot}";

        switch (payload) {
            case GearsetPayload { GlamourPlateId: >= 1 and <= MaxGlamourPlateId } p:
                command += $" {p.GlamourPlateId}";
                break;
            case GearsetPayload { GlamourPlateId: not null }:
                throw new ActionInvalidException(
                    string.Format(UIStrings.GearsetStrategy_InvalidGlamourPlateError, MaxGlamourPlateId));
        }

        pluginLog.Debug("Executing command: {Command}", command);
        await framework.RunOnFrameworkThread(() => { ChatHelper.SendSanitizedChatMessage(command); });
    }

    public Task<ActionAppearance> GetAppearance(uint actionSlot) {
        var gearset = GetGearsetBySlot(actionSlot) ??
                      throw new ActionNotFoundException(string.Format(UIStrings.GearsetStrategy_GearsetNotFoundError, actionSlot));

        // except the game wants a zero-indexed gearset.
        return appearanceResolver.GetActionAppearance(HotbarSlotType.GearSet, (uint)(gearset.Slot - 1));
    }

    public Type GetPayloadType() => typeof(GearsetPayload);
}

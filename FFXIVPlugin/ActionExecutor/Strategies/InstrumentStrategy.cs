using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using static FFXIVClientStructs.FFXIV.Client.UI.Misc.RaptureHotbarModule;
using Lumina.Excel;
using Lumina.Excel.Sheets;
using XIVDeck.FFXIVPlugin.ActionExecutor.Payloads;
using XIVDeck.FFXIVPlugin.Exceptions;
using XIVDeck.FFXIVPlugin.Game.Managers;
using XIVDeck.FFXIVPlugin.Resources.Localization;
using XIVDeck.FFXIVPlugin.Game;
using ActionAppearance = XIVDeck.FFXIVPlugin.Contract.ActionAppearance;
using XIVDeck.FFXIVPlugin.Contract;

namespace XIVDeck.FFXIVPlugin.ActionExecutor.Strategies;

[ActionStrategy(HotbarSlotType.PerformanceInstrument)]
public class InstrumentStrategy(IDataManager dataManager, ICondition condition, IFramework framework,
    ActionAppearanceResolver appearanceResolver) : IActionStrategy {
    private readonly ExcelSheet<Perform> _performSheet = dataManager.Excel.GetSheet<Perform>();

    private static ActionEntry GetExecutableAction(Perform instrument) {
        return new ActionEntry {
            Id = (int) instrument.RowId,
            Name = instrument.Instrument.ToString(),
            Type = HotbarSlotType.PerformanceInstrument,
            SortOrder = instrument.Icon
        };
    }

    private Perform? GetActionById(uint id) {
        return this._performSheet.GetRowOrDefault(id);
    }

    private static unsafe bool IsPerformUnlocked() {
        return UIState.Instance()->IsUnlockLinkUnlocked(255);
    }

    public ActionEntry? GetActionEntryById(uint actionId) {
        var action = this.GetActionById(actionId);
        return action == null ? null : GetExecutableAction(action.Value);
    }

    public List<ActionEntry> GetSelectableActions() {
        if (!IsPerformUnlocked()) {
            return [];
        }

        return [.. this._performSheet.Where(i => i.RowId > 0).Select(GetExecutableAction)];
    }

    public async Task Execute(uint actionId, ActionPayload? _) {
        // intentionally not checking for Bard here; the game will take care of that for us (and display a better
        // error than we normally can). It's legal for a perform to be on a non-Bard hotbar, so I'm not concerned
        // about this.

        if (!IsPerformUnlocked()) {
            throw new ActionLockedException(UIStrings.InstrumentStrategy_PerformanceLockedError);
        }

        if (condition[ConditionFlag.Performing]) {
            throw new IllegalGameStateException(UIStrings.InstrumentStrategy_CurrentlyPerformingError);
        }

        var instrument = this.GetActionById(actionId);

        if (instrument == null) {
            throw new ActionNotFoundException(string.Format(UIStrings.InstrumentStrategy_InstrumentNotFoundError, actionId));
        }

        await framework.RunOnFrameworkThread(delegate {
            HotbarManager.ExecuteHotbarAction(HotbarSlotType.PerformanceInstrument, actionId);
        });
    }

    public Task<ActionAppearance> GetAppearance(uint actionId) =>
        appearanceResolver.GetActionAppearance(HotbarSlotType.PerformanceInstrument, actionId);
}

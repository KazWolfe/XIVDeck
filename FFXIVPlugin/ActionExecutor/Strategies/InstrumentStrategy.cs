using System;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Conditions;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using static FFXIVClientStructs.FFXIV.Client.UI.Misc.RaptureHotbarModule;
using Lumina.Excel;
using Lumina.Excel.Sheets;
using XIVDeck.FFXIVPlugin.Base;
using XIVDeck.FFXIVPlugin.Exceptions;
using XIVDeck.FFXIVPlugin.Game.Managers;
using XIVDeck.FFXIVPlugin.Resources.Localization;

namespace XIVDeck.FFXIVPlugin.ActionExecutor.Strategies;

[ActionStrategy(HotbarSlotType.PerformanceInstrument)]
public class InstrumentStrategy : IActionStrategy {
    private static readonly ExcelSheet<Perform> PerformSheet = Injections.DataManager.Excel.GetSheet<Perform>()!;

    private static ExecutableAction GetExecutableAction(Perform instrument) {
        return new ExecutableAction {
            ActionId = (int) instrument.RowId,
            ActionName = instrument.Instrument.ToString(),
            IconId = instrument.Order,
            HotbarSlotType = HotbarSlotType.PerformanceInstrument,
            SortOrder = instrument.Order
        };
    }

    private static Perform? GetActionById(uint id) {
        return PerformSheet.GetRow(id);
    }

    private static unsafe bool IsPerformUnlocked() {
        return UIState.Instance()->IsUnlockLinkUnlocked(255);
    }

    public ExecutableAction? GetExecutableActionById(uint actionId) {
        var action = GetActionById(actionId);
        return action == null ? null : GetExecutableAction(action.Value);
    }

    public List<ExecutableAction>? GetAllowedItems() {
        return !IsPerformUnlocked() ? null : PerformSheet.Where(i => i.RowId > 0).Select(GetExecutableAction).ToList();
    }

    public void Execute(uint actionId, ActionPayload? _) {
        // intentionally not checking for Bard here; the game will take care of that for us (and display a better
        // error than we normally can). It's legal for a perform to be on a non-Bard hotbar, so I'm not concerned
        // about this.

        if (!IsPerformUnlocked()) {
            throw new ActionLockedException(UIStrings.InstrumentStrategy_PerformanceLockedError);
        }

        if (Injections.Condition[ConditionFlag.Performing]) {
            throw new IllegalGameStateException(UIStrings.InstrumentStrategy_CurrentlyPerformingError);
        }

        var instrument = GetActionById(actionId);

        if (instrument == null) {
            throw new ArgumentOutOfRangeException(nameof(actionId), string.Format(UIStrings.InstrumentStrategy_InstrumentNotFoundError, actionId));
        }

        Injections.Framework.RunOnFrameworkThread(delegate {
            HotbarManager.ExecuteHotbarAction(HotbarSlotType.PerformanceInstrument, actionId);
        });
    }

    public int GetIconId(uint actionId) {
        return GetActionById(actionId)?.Order ?? 0;
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Conditions;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using Lumina.Excel;
using Lumina.Excel.GeneratedSheets;
using XIVDeck.FFXIVPlugin.Base;
using XIVDeck.FFXIVPlugin.Exceptions;
using XIVDeck.FFXIVPlugin.Game;
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
            HotbarSlotType = HotbarSlotType.PerformanceInstrument
        };
    }

    private static Perform? GetActionById(uint id) {
        return PerformSheet.GetRow(id);
    }

    public unsafe bool IsPerformUnlocked() {
        // APPARENTLY unlock 255 is performance?!
        return XIVDeckPlugin.Instance.SigHelper.IsQuestCompleted(68555) && UIState.Instance()->IsUnlockLinkUnlocked(255);
    }

    public ExecutableAction? GetExecutableActionById(uint actionId) {
        var action = GetActionById(actionId);
        return action == null ? null : GetExecutableAction(action);
    }

    public List<ExecutableAction>? GetAllowedItems() {
        return !this.IsPerformUnlocked() ? null : PerformSheet.Where(i => i.RowId > 0).Select(GetExecutableAction).ToList();
    }

    public void Execute(uint actionId, dynamic? _) {
        // intentionally not checking for Bard here; the game will take care of that for us (and display a better
        // error than we normally can). It's legal for a perform to be on a non-Bard hotbar, so I'm not concerned
        // about this.
            
        if (!this.IsPerformUnlocked()) {
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
            GameUtils.ExecuteHotbarAction(HotbarSlotType.PerformanceInstrument, actionId);
        });
    }

    public int GetIconId(uint actionId) {
        return GetActionById(actionId)?.Order ?? 0;
    }
}
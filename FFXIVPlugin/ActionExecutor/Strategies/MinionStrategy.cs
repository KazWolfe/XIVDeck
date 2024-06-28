using System;
using System.Collections.Generic;
using System.Linq;
using static FFXIVClientStructs.FFXIV.Client.UI.Misc.RaptureHotbarModule;
using Lumina.Excel.GeneratedSheets;
using XIVDeck.FFXIVPlugin.Base;
using XIVDeck.FFXIVPlugin.Exceptions;
using XIVDeck.FFXIVPlugin.Game;
using XIVDeck.FFXIVPlugin.Game.Managers;
using XIVDeck.FFXIVPlugin.Resources.Localization;
using XIVDeck.FFXIVPlugin.Utils;

namespace XIVDeck.FFXIVPlugin.ActionExecutor.Strategies;

[ActionStrategy(HotbarSlotType.Companion)]
public class MinionStrategy : IActionStrategy {
    private static ExecutableAction GetExecutableAction(Companion minion) {
        return new ExecutableAction {
            ActionId = (int) minion.RowId,
            ActionName = minion.Singular.ToString(),
            IconId = minion.Icon,
            HotbarSlotType = HotbarSlotType.Companion,
            SortOrder = minion.Order
        };
    }

    private static Companion? GetMinionById(uint id) {
        return Injections.DataManager.Excel.GetSheet<Companion>()!.GetRow(id);
    }

    public List<ExecutableAction> GetAllowedItems() {
        return Injections.DataManager.GetExcelSheet<Companion>()!
            .Where(c => c.IsUnlocked())
            .Select(GetExecutableAction)
            .ToList();
    }

    public ExecutableAction? GetExecutableActionById(uint actionId) {
        var action = GetMinionById(actionId);
        return action == null ? null : GetExecutableAction(action);
    }

    public void Execute(uint actionId, ActionPayload? _) {
        var minion = GetMinionById(actionId);

        if (minion == null) {
            throw new ArgumentNullException(nameof(actionId), string.Format(UIStrings.MinionStrategy_MinionNotFoundError, actionId));
        }

        if (!minion.IsUnlocked()) {
            throw new ActionLockedException(string.Format(UIStrings.MinionStrategy_MinionLockedError, minion.Singular.ToTitleCase()));
        }

        Injections.PluginLog.Debug($"Executing hotbar slot: Minion#{actionId} ({minion.Singular.ToTitleCase()})");
        Injections.Framework.RunOnFrameworkThread(delegate {
            HotbarManager.ExecuteHotbarAction(HotbarSlotType.Companion, actionId);
        });
    }

    public int GetIconId(uint item) {
        return GetMinionById(item)?.Icon ?? 0;
    }
}

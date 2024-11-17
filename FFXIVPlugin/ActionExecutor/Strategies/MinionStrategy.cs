using System;
using System.Collections.Generic;
using System.Linq;
using Lumina.Excel.Sheets;
using static FFXIVClientStructs.FFXIV.Client.UI.Misc.RaptureHotbarModule;
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
        return Injections.DataManager.Excel.GetSheet<Companion>().GetRowOrDefault(id);
    }

    public List<ExecutableAction> GetAllowedItems() {
        return Injections.DataManager.GetExcelSheet<Companion>()
            .Where(c => c.IsUnlocked())
            .Select(GetExecutableAction)
            .ToList();
    }

    public ExecutableAction? GetExecutableActionById(uint actionId) {
        var action = GetMinionById(actionId);
        return action == null ? null : GetExecutableAction(action.Value);
    }

    public void Execute(uint actionId, ActionPayload? _) {
        var minion = GetMinionById(actionId);

        if (minion == null) {
            throw new ArgumentNullException(nameof(actionId), string.Format(UIStrings.MinionStrategy_MinionNotFoundError, actionId));
        }

        if (!minion.Value.IsUnlocked()) {
            throw new ActionLockedException(string.Format(UIStrings.MinionStrategy_MinionLockedError, minion.Value.Singular.ToTitleCase()));
        }

        Injections.PluginLog.Debug($"Executing hotbar slot: Minion#{actionId} ({minion.Value.Singular.ToTitleCase()})");
        Injections.Framework.RunOnFrameworkThread(delegate {
            HotbarManager.ExecuteHotbarAction(HotbarSlotType.Companion, actionId);
        });
    }

    public int GetIconId(uint item) {
        return GetMinionById(item)?.Icon ?? 0;
    }
}

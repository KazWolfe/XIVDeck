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

[ActionStrategy(HotbarSlotType.Mount)]
public class MountStrategy : IActionStrategy {
    private static ExecutableAction GetExecutableAction(Mount mount) {
        return new ExecutableAction {
            ActionId = (int) mount.RowId,
            ActionName = mount.Singular.ToString(),
            IconId = mount.Icon,
            HotbarSlotType = HotbarSlotType.Mount,
            SortOrder = (mount.UIPriority << 8) + mount.Order
        };
    }

    private static Mount? GetMountById(uint id) {
        return Injections.DataManager.Excel.GetSheet<Mount>()!.GetRow(id);
    }

    public List<ExecutableAction> GetAllowedItems() {
        return Injections.DataManager.GetExcelSheet<Mount>()!
            .Where(m => m.IsUnlocked())
            .Select(GetExecutableAction)
            .ToList();
    }

    public void Execute(uint actionId, ActionPayload? _) {
        var mount = GetMountById(actionId);

        if (mount == null) {
            throw new ArgumentNullException(nameof(actionId), string.Format(UIStrings.MountStrategy_MountNotFoundError, actionId));
        }

        if (!mount.Value.IsUnlocked()) {
            throw new ActionLockedException(string.Format(UIStrings.MountStrategy_MountLockedError, mount.Value.Singular.ToTitleCase()));
        }

        Injections.PluginLog.Debug($"Executing hotbar slot: Mount#{actionId} ({mount.Value.Singular.ToTitleCase()})");
        Injections.Framework.RunOnFrameworkThread(delegate {
            HotbarManager.ExecuteHotbarAction(HotbarSlotType.Mount, actionId);
        });
    }

    public ExecutableAction? GetExecutableActionById(uint actionId) {
        var action = GetMountById(actionId);
        return action == null ? null : GetExecutableAction(action.Value);
    }

    public int GetIconId(uint item) {
        return GetMountById(item)?.Icon ?? 0;

    }
}

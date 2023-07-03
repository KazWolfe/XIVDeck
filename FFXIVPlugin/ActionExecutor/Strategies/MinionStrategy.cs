using System;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Logging;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using Lumina.Excel.GeneratedSheets;
using XIVDeck.FFXIVPlugin.Base;
using XIVDeck.FFXIVPlugin.Exceptions;
using XIVDeck.FFXIVPlugin.Game;
using XIVDeck.FFXIVPlugin.Game.Chat;
using XIVDeck.FFXIVPlugin.Resources.Localization;
using XIVDeck.FFXIVPlugin.Utils;

namespace XIVDeck.FFXIVPlugin.ActionExecutor.Strategies; 

[ActionStrategy(HotbarSlotType.Minion)]
public class MinionStrategy : IActionStrategy {
    private static ExecutableAction GetExecutableAction(Companion minion) {
        return new ExecutableAction {
            ActionId = (int) minion.RowId,
            ActionName = minion.Singular.ToString(),
            IconId = minion.Icon,
            HotbarSlotType = HotbarSlotType.Minion,
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
            
        var command = $"/minion \"{minion.Singular}\"";
            
        PluginLog.Debug($"Executing command: {command}");
        Injections.Framework.RunOnFrameworkThread(delegate {
            ChatHelper.GetInstance().SendSanitizedChatMessage(command);
        });
    }

    public int GetIconId(uint item) {
        return GetMinionById(item)?.Icon ?? 0;
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Logging;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using Lumina.Excel.GeneratedSheets;
using XIVDeck.FFXIVPlugin.Base;
using XIVDeck.FFXIVPlugin.Exceptions;
using XIVDeck.FFXIVPlugin.Game;
using XIVDeck.FFXIVPlugin.Resources.Localization;
using XIVDeck.FFXIVPlugin.Utils;

namespace XIVDeck.FFXIVPlugin.ActionExecutor.Strategies; 

[ActionStrategy(HotbarSlotType.Minion)]
public class MinionStrategy : IActionStrategy {
    private static readonly GameStateCache GameStateCache = XIVDeckPlugin.Instance.GameStateCache;

    private static ExecutableAction GetExecutableAction(Companion minion) {
        return new ExecutableAction {
            ActionId = (int) minion.RowId,
            ActionName = minion.Singular.ToString(),
            IconId = minion.Icon,
            HotbarSlotType = HotbarSlotType.Minion
        };
    }
        
    private static Companion? GetMinionById(uint id) {
        return Injections.DataManager.Excel.GetSheet<Companion>()!.GetRow(id);
    }
        
    public List<ExecutableAction> GetAllowedItems() {
        GameStateCache.Refresh();
        return GameStateCache.UnlockedMinions!.Select(GetExecutableAction).ToList();
    }

    public ExecutableAction? GetExecutableActionById(uint actionId) {
        var action = GetMinionById(actionId);
        return action == null ? null : GetExecutableAction(action);
    }

    public void Execute(uint actionId, dynamic? _) {
        var minion = GetMinionById(actionId);
            
        if (minion == null) {
            throw new ArgumentNullException(nameof(actionId), string.Format(UIStrings.MinionStrategy_MinionNotFoundError, actionId));
        }

        if (!GameStateCache.IsMinionUnlocked(actionId)) {
            throw new ActionLockedException(string.Format(UIStrings.MinionStrategy_MinionLockedError, minion.Singular));
        }
            
        var command = $"/minion \"{minion.Singular}\"";
            
        PluginLog.Debug($"Would execute command: {command}");
        TickScheduler.Schedule(delegate {
            GameUtils.SendSanitizedChatMessage(command);
        });
    }

    public int GetIconId(uint item) {
        return GetMinionById(item)?.Icon ?? 0;
    }
}
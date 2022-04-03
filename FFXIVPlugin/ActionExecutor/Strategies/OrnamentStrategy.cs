using System;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Logging;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using Lumina.Excel.GeneratedSheets;
using XIVDeck.FFXIVPlugin.Base;
using XIVDeck.FFXIVPlugin.Exceptions;
using XIVDeck.FFXIVPlugin.Game;
using XIVDeck.FFXIVPlugin.Utils;

namespace XIVDeck.FFXIVPlugin.ActionExecutor.Strategies; 

public class OrnamentStrategy : IActionStrategy {
    private static readonly GameStateCache GameStateCache = XIVDeckPlugin.Instance.GameStateCache;

    private static ExecutableAction GetExecutableAction(Ornament ornament) {
        return new ExecutableAction {
            ActionId = (int) ornament.RowId,
            ActionName = ornament.Singular.ToString(),
            IconId = ornament.Icon,
            HotbarSlotType = HotbarSlotType.FashionAccessory
        };
    }
        
    private static Ornament? GetOrnamentById(uint id) {
        return Injections.DataManager.Excel.GetSheet<Ornament>()!.GetRow(id);
    }
        
    public List<ExecutableAction> GetAllowedItems() {
        GameStateCache.Refresh();
            
        return GameStateCache.UnlockedOrnaments!.Select(GetExecutableAction).ToList();
    }

    public ExecutableAction? GetExecutableActionById(uint actionId) {
        var action = GetOrnamentById(actionId);
        return action == null ? null : GetExecutableAction(action);
    }

    public void Execute(uint actionId, dynamic? _) {
        Ornament? ornament = GetOrnamentById(actionId);

        if (ornament == null) {
            throw new ActionNotFoundException(HotbarSlotType.FashionAccessory, actionId);
        }

        if (!GameStateCache.IsOrnamentUnlocked(actionId)) {
            throw new ActionLockedException($"The fashion accessory \"{ornament.Singular}\" isn't unlocked and therefore can't be used.");
        }
            
        String command = $"/fashion \"{ornament.Singular}\"";
        PluginLog.Debug($"Would execute command: {command}");
        TickScheduler.Schedule(delegate {
            ChatUtils.SendSanitizedChatMessage(command);
        });
    }

    public int GetIconId(uint item) {
        return GetOrnamentById(item)?.Icon ?? 0;
    }
}
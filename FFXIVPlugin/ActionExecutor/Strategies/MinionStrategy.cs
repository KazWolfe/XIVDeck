using System;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Logging;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using FFXIVPlugin.Base;
using FFXIVPlugin.helpers;
using Lumina.Excel.GeneratedSheets;

namespace FFXIVPlugin.ActionExecutor.Strategies {
    public class MinionStrategy : IBaseStrategy {
        public Companion GetMinionById(uint id) {
            return Injections.DataManager.Excel.GetSheet<Companion>().GetRow(id);
        }
        
        public List<ExecutableAction> GetAllowedItems() {
            GameStateCache gameStateCache = XIVDeckPlugin.Instance.GameStateCache;
            gameStateCache.Refresh();

            return gameStateCache.UnlockedMinionKeys.Select(minion => new ExecutableAction() {
                ActionId = (int) minion.RowId, 
                ActionName = minion.Singular.RawString, 
                HotbarSlotType = HotbarSlotType.Minion
            }).ToList();
        }

        public void Execute(uint actionId, dynamic _) {
            Companion minion = GetMinionById(actionId);
            
            String command = $"/minion \"{minion.Singular.RawString}\"";
            
            PluginLog.Debug($"Would execute command: {command}");
            // XIVDeckPlugin.Instance.XivCommon.Functions.Chat.SendMessage(command);
        }

        public int GetIconId(uint item) {
            Companion minion = GetMinionById(item);
            return minion.Icon;
        }
    }
}
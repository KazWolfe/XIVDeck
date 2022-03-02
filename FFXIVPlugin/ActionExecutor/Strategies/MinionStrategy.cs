using System;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Logging;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using FFXIVPlugin.Base;
using Lumina.Excel.GeneratedSheets;

namespace FFXIVPlugin.ActionExecutor.Strategies {
    public class MinionStrategy : IStrategy {
        private static readonly GameStateCache GameStateCache = XIVDeckPlugin.Instance.GameStateCache;
        
        public Companion GetMinionById(uint id) {
            return Injections.DataManager.Excel.GetSheet<Companion>()!.GetRow(id);
        }
        
        public List<ExecutableAction> GetAllowedItems() {
            GameStateCache.Refresh();

            return GameStateCache.UnlockedMinionKeys!.Select(minion => new ExecutableAction() {
                ActionId = (int) minion.RowId, 
                ActionName = minion.Singular.RawString, 
                HotbarSlotType = HotbarSlotType.Minion
            }).ToList();
        }

        public void Execute(uint actionId, dynamic _) {
            Companion minion = this.GetMinionById(actionId);

            if (!GameStateCache.IsMinionUnlocked(actionId)) {
                throw new InvalidOperationException($"The minion \"{minion.Singular.RawString}\" isn't unlocked and therefore can't be used.");
            }
            
            String command = $"/minion \"{minion.Singular.RawString}\"";
            
            PluginLog.Debug($"Would execute command: {command}");
            
            XIVDeckPlugin.Instance.XivCommon.Functions.Chat.SendMessage(command);
        }

        public int GetIconId(uint item) {
            Companion minion = this.GetMinionById(item);
            return minion.Icon;
        }
    }
}
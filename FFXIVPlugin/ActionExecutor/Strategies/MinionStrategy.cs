using System;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Logging;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using FFXIVPlugin.Base;
using Lumina.Excel.GeneratedSheets;

namespace FFXIVPlugin.ActionExecutor.Strategies {
    public class MinionStrategy : IStrategy {
        private static GameStateCache _gameStateCache = XIVDeckPlugin.Instance.GameStateCache;
        
        public Companion GetMinionById(uint id) {
            return Injections.DataManager.Excel.GetSheet<Companion>()!.GetRow(id);
        }
        
        public List<ExecutableAction> GetAllowedItems() {
            _gameStateCache.Refresh();

            return _gameStateCache.UnlockedMinionKeys!.Select(minion => new ExecutableAction() {
                ActionId = (int) minion.RowId, 
                ActionName = minion.Singular.RawString, 
                HotbarSlotType = HotbarSlotType.Minion
            }).ToList();
        }

        public void Execute(uint actionId, dynamic _) {
            Companion minion = GetMinionById(actionId);

            if (!_gameStateCache.IsMinionUnlocked(actionId)) {
                throw new InvalidOperationException($"The minion \"{minion.Singular.RawString}\" isn't unlocked and therefore can't be used.");
            }
            
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
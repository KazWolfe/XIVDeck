using System;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Logging;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using FFXIVPlugin.Base;
using Lumina.Excel.GeneratedSheets;

namespace FFXIVPlugin.ActionExecutor.Strategies {
    public class MountStrategy : IStrategy {
        private static GameStateCache _gameStateCache = XIVDeckPlugin.Instance.GameStateCache;
        
        public Mount GetMountById(uint id) {
            return Injections.DataManager.Excel.GetSheet<Mount>()!.GetRow(id);
        }
        
        public List<ExecutableAction> GetAllowedItems() {
            _gameStateCache.Refresh();

            return _gameStateCache.UnlockedMountKeys!.Select(mount => new ExecutableAction() {
                ActionId = (int) mount.RowId, 
                ActionName = mount.Singular.RawString, 
                HotbarSlotType = HotbarSlotType.Mount
            }).ToList();
        }

        public void Execute(uint actionId, dynamic _) {
            Mount mount = this.GetMountById(actionId);
            
            if (!_gameStateCache.IsMountUnlocked(actionId)) {
                throw new InvalidOperationException($"The mount \"{mount.Singular.RawString}\" isn't unlocked and therefore can't be used.");
            }
            
            String command = $"/mount \"{mount.Singular.RawString}\"";
            
            PluginLog.Debug($"Would execute command: {command}");
            XIVDeckPlugin.Instance.XivCommon.Functions.Chat.SendMessage(command);
        }

        public int GetIconId(uint item) {
            Mount mount = GetMountById(item);
            return mount.Icon;
        }
    }
}
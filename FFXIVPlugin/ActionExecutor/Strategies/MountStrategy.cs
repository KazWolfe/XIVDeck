using System;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Logging;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using FFXIVPlugin.Base;
using FFXIVPlugin.helpers;
using Lumina.Excel.GeneratedSheets;

namespace FFXIVPlugin.ActionExecutor.Strategies {
    public class MountStrategy : IStrategy {
        public Mount GetMountById(uint id) {
            return Injections.DataManager.Excel.GetSheet<Mount>().GetRow(id);
        }
        
        public List<ExecutableAction> GetAllowedItems() {
            GameStateCache gameStateCache = XIVDeckPlugin.Instance.GameStateCache;
            gameStateCache.Refresh();

            return gameStateCache.UnlockedMountKeys.Select(mount => new ExecutableAction() {
                ActionId = (int) mount.RowId, 
                ActionName = mount.Singular.RawString, 
                HotbarSlotType = HotbarSlotType.Mount
            }).ToList();
        }

        public void Execute(uint actionId, dynamic _) {
            Mount mount = GetMountById(actionId);
            
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
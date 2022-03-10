using System;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Logging;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using Lumina.Excel.GeneratedSheets;
using XIVDeck.FFXIVPlugin.Base;

namespace XIVDeck.FFXIVPlugin.ActionExecutor.Strategies {
    public class OrnamentStrategy : IStrategy {
        private static readonly GameStateCache GameStateCache = XIVDeckPlugin.Instance.GameStateCache;
        
        public Ornament GetOrnamentById(uint id) {
            return Injections.DataManager.Excel.GetSheet<Ornament>()!.GetRow(id);
        }
        
        public List<ExecutableAction> GetAllowedItems() {
            GameStateCache.Refresh();
            
            return GameStateCache.UnlockedOrnamentKeys!.Select(ornament => new ExecutableAction() {
                ActionId = (int) ornament.RowId, 
                ActionName = ornament.Singular.RawString, 
                HotbarSlotType = HotbarSlotType.FashionAccessory
            }).ToList();
        }

        public void Execute(uint actionId, dynamic _) {
            Ornament ornament = this.GetOrnamentById(actionId);

            if (!GameStateCache.IsOrnamentUnlocked(actionId)) {
                throw new InvalidOperationException($"The fashion accessory \"{ornament.Singular.RawString}\" isn't unlocked and therefore can't be used.");
            }
            
            String command = $"/fashion \"{ornament.Singular.RawString}\"";
            
            PluginLog.Debug($"Would execute command: {command}");
            XIVDeckPlugin.Instance.XivCommon.Functions.Chat.SendMessage(command);
        }

        public int GetIconId(uint item) {
            Ornament ornament = this.GetOrnamentById(item);
            return ornament.Icon;
        }
    }
}
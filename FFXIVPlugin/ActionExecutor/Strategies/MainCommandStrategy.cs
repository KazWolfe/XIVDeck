using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using FFXIVPlugin.Base;
using Lumina.Excel.GeneratedSheets;

namespace FFXIVPlugin.ActionExecutor.Strategies {
    public class MainCommandStrategy : IStrategy {
        private readonly List<MainCommand> _mainCommandCache = new();
        
        public MainCommandStrategy() {
            // initialize the MainCommand cache with valid action types as part of construction.
            // this value is fixed so this is safe-ish.

            var sheet = Injections.DataManager.Excel.GetSheet<MainCommand>();

            Debug.Assert(sheet != null, nameof(sheet) + " != null");
            
            foreach (var command in sheet) {
                // cheap but effective test; this ignores basically anything that's not in a menu
                if (command.Category == 0) continue;

                this._mainCommandCache.Add(command);
            }
        }
        
        
        public unsafe void Execute(uint actionId, dynamic options = null) {
            if (this._mainCommandCache.All(command => actionId != command.RowId))
                throw new InvalidOperationException($"Main command action ID {actionId} is not valid.");
            
            Framework.Instance()->GetUiModule()->ExecuteMainCommand(actionId);
        }

        public int GetIconId(uint actionId) {
            return Injections.DataManager.Excel.GetSheet<MainCommand>()!.GetRow(actionId)!.Icon;
        }

        public List<ExecutableAction> GetAllowedItems() {
            return this._mainCommandCache.Select(command => new ExecutableAction() {
                ActionId = (int) command.RowId, 
                ActionName = command.Name.RawString, 
                HotbarSlotType = HotbarSlotType.MainCommand
            }).ToList();
        }
    }
}
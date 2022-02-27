using System;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Logging;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using FFXIVPlugin.Base;
using Lumina.Excel;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace FFXIVPlugin.ActionExecutor {
    public class ExecutableAction {
        [JsonProperty("name")] public string ActionName; // optional, will realistically only ever be sent
        [JsonProperty("id")] public int ActionId;
        
        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty("type")] public HotbarSlotType HotbarSlotType;
    }
    
    public interface IStrategy {

        /**
         * Execute an event with the given Action ID, depending on the strategy for this action type.
         */
        public void Execute(uint actionId, dynamic options = null);

        /**
         * Get the Icon ID used for a specific action type
         */
        public int GetIconId(uint actionId);

        /**
         * Get a dynamic list of items allowed by this strategy.
         * 
         */
        public List<ExecutableAction> GetAllowedItems();
    }

    public abstract class FixedCommandStrategy<T> : IStrategy where T : ExcelRow {
        private List<ExecutableAction> _actionCache = new();

        protected abstract string GetNameForAction(T action);
        protected abstract HotbarSlotType GetHotbarSlotType();
        protected abstract int GetIconForAction(T action);
        protected abstract string GetCommandToCallAction(T action);

        protected virtual uint[] GetIllegalActionIDs() {
            return new uint[] { };
        }

        public T GetActionById(uint id) {
            return Injections.DataManager.Excel.GetSheet<T>().GetRow(id);
        }

        public List<ExecutableAction> GetAllowedItems() {
            if (this._actionCache.Count > 0) {
                // this is (relatively) safe as general actions shouldn't (can't) be added midway through the game.
                // so let's just cache them and return whenever this is called just to save a tiiiny amount of memory
                return this._actionCache;
            }

            ExcelSheet<T> sheet = Injections.DataManager.Excel.GetSheet<T>();

            if (sheet == null) {
                throw new NullReferenceException($"A sheet of type {typeof(T).Name} does not exist.");
            }
            
            foreach (var row in sheet) {
                // skip illegal action IDs
                if (row.RowId == 0) continue;
                if (this.GetIllegalActionIDs().Contains(row.RowId)) continue;

                var actionName = GetNameForAction(row);
                if (string.IsNullOrEmpty(actionName)) continue;

                this._actionCache.Add(new ExecutableAction() {
                    ActionId = (int) row.RowId,
                    ActionName = actionName,
                    HotbarSlotType = GetHotbarSlotType()
                });
            }

            return this._actionCache;
        }
        
        public void Execute(uint actionId, dynamic options = null) {
            if (this.GetIllegalActionIDs().Contains(actionId))
                throw new ArgumentException($"The action with ID {actionId} is marked as illegal and cannot be used.");
            
            String command = this.GetCommandToCallAction(this.GetActionById(actionId));
            
            PluginLog.Debug($"Would execute command: {command}");
            XIVDeckPlugin.Instance.XivCommon.Functions.Chat.SendMessage(command);
        }

        public int GetIconId(uint actionId) {
            return this.GetIconForAction(this.GetActionById(actionId));
        }
    }
}
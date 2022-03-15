using System;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Logging;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using Lumina.Excel;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using XIVDeck.FFXIVPlugin.Base;
using XIVDeck.FFXIVPlugin.Utils;

namespace XIVDeck.FFXIVPlugin.ActionExecutor {
    public class ExecutableAction {
        [JsonProperty("name")] public string? ActionName; // optional, will realistically only ever be sent
        [JsonProperty("id")] public int ActionId;

        [JsonProperty("category")] public string? Category; // optional, send-only. used for grouping where available
        
        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty("type")] public HotbarSlotType HotbarSlotType;
    }
    
    public interface IStrategy {

        /**
         * Execute an event with the given Action ID, depending on the strategy for this action type.
         */
        public void Execute(uint actionId, dynamic? options = null);

        /**
         * Get the Icon ID used for a specific action type
         */
        public int GetIconId(uint actionId);

        /**
         * Get a dynamic list of items allowed by this strategy.
         * 
         */
        public List<ExecutableAction>? GetAllowedItems();
    }

    public abstract class FixedCommandStrategy<T> : IStrategy where T : ExcelRow {
        private List<ExecutableAction> _actionCache = new();

        protected abstract string GetNameForAction(T action);
        protected abstract HotbarSlotType GetHotbarSlotType();
        protected abstract int GetIconForAction(T action);
        protected abstract string? GetCommandToCallAction(T action);

        protected virtual IEnumerable<uint> GetIllegalActionIDs() {
            return Array.Empty<uint>();
        }

        public T? GetActionById(uint id) {
            // should never be null, T is inherently handled by Lumina
            return Injections.DataManager.Excel.GetSheet<T>()!.GetRow(id);
        }

        public List<ExecutableAction> GetAllowedItems() {
            if (this._actionCache.Count > 0) {
                // this is (relatively) safe as general actions shouldn't (can't) be added midway through the game.
                // so let's just cache them and return whenever this is called just to save a tiiiny amount of memory
                return this._actionCache;
            }

            ExcelSheet<T> sheet = Injections.DataManager.Excel.GetSheet<T>()!;

            if (sheet == null) {
                throw new NullReferenceException($"A sheet of type {typeof(T).Name} does not exist.");
            }
            
            foreach (var row in sheet) {
                // skip illegal action IDs
                if (row.RowId == 0) continue;
                if (this.GetIllegalActionIDs().Contains(row.RowId)) continue;

                var actionName = this.GetNameForAction(row);
                if (string.IsNullOrEmpty(actionName)) continue;

                this._actionCache.Add(new ExecutableAction() {
                    ActionId = (int) row.RowId,
                    ActionName = actionName,
                    HotbarSlotType = this.GetHotbarSlotType()
                });
            }

            return this._actionCache;
        }
        
        public void Execute(uint actionId, dynamic? options = null) {
            if (this.GetIllegalActionIDs().Contains(actionId))
                throw new ArgumentException($"The action with ID {actionId} is marked as illegal and cannot be used.");

            var action = this.GetActionById(actionId);

            if (action == null) {
                throw new ArgumentNullException(nameof(actionId), $"An action of type {typeof(T)} with ID {actionId} does not exist.");
            }
            
            String? command = this.GetCommandToCallAction(action);

            if (command == null) {
                PluginLog.Warning("An ExecutableAction returned without a command. This shouldn't happen, but isn't fatal either.");
                return;
            }

            PluginLog.Debug($"Would execute command: {command}");

            TickScheduler.Schedule(delegate {
                ChatUtil.SendSanitizedChatMessage(command);
            });
        }

        public int GetIconId(uint actionId) {
            var action = this.GetActionById(actionId);

            return action == null ? 0 : this.GetIconForAction(action);
        }
    }
}
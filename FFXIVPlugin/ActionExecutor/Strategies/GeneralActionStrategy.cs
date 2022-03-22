using System;
using System.Collections.Generic;
using System.Linq;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using Lumina.Excel;
using Lumina.Excel.GeneratedSheets;
using XIVDeck.FFXIVPlugin.Base;
using XIVDeck.FFXIVPlugin.Exceptions;
using XIVDeck.FFXIVPlugin.Utils;

namespace XIVDeck.FFXIVPlugin.ActionExecutor.Strategies {
    public class GeneralActionStrategy : IActionStrategy {
        private List<uint> _illegalActionCache = new();
        private static readonly ExcelSheet<GeneralAction> ActionSheet = Injections.DataManager.Excel.GetSheet<GeneralAction>()!;

        private static ExecutableAction GetExecutableAction(GeneralAction action) {
            return new ExecutableAction {
                ActionId = (int) action.RowId,
                ActionName = action.Name.RawString,
                IconId = action.Icon,
                HotbarSlotType = HotbarSlotType.GeneralAction
            };
        }
        
        private static GeneralAction? GetActionById(uint actionId) {
            return ActionSheet.GetRow(actionId);
        }

        private IEnumerable<uint> GetIllegalActionIDs() {
            if (this._illegalActionCache.Count > 0) {
                return this._illegalActionCache;
            }
            
            var illegalActions = new List<uint> {
                13, // Advanced Materia Melding - automatically injected on use of Materia Melding
            };

            foreach (var action in ActionSheet) {
                if (illegalActions.Contains(action.RowId)) continue;
                
                // empty or non-ui actions are considered illegal
                if (action.UIPriority == 0 || action.Name.RawString == "") {
                    illegalActions.Add(action.RowId);
                }
            }

            this._illegalActionCache = illegalActions;
            return illegalActions;
        }

        public unsafe ExecutableAction? GetExecutableActionById(uint actionId) {
            var action = GetActionById(actionId);

            if (action == null) return null;
            
            // ERRATA - swap out melding
            if (actionId == 12 && UIState.Instance()->Hotbar.IsActionUnlocked(12)) {
                action = GetActionById(13)!;
            }

            return GetExecutableAction(action);
        }

        public unsafe void Execute(uint actionId, dynamic? _) {
            var action = GetActionById(actionId);
            
            if (action == null) {
                throw new ArgumentOutOfRangeException(nameof(actionId), $"No action with ID {actionId} exists.");
            }
            
            if (this.GetIllegalActionIDs().Contains(actionId)) {
                throw new ArgumentOutOfRangeException(nameof(actionId),
                    $"The action \"{action.Name.RawString}\" (ID {actionId}) is marked as illegal and cannot be used.");
            }
            
            if (action.UnlockLink != 0 && !UIState.Instance()->Hotbar.IsActionUnlocked(action.UnlockLink)) {
                throw new ActionLockedException($"The action \"{action.Name.RawString}\" is not yet unlocked.");
            }
            
            if (actionId == 12 && UIState.Instance()->Hotbar.IsActionUnlocked(12)) {
                action = GetActionById(13)!;
            }

            TickScheduler.Schedule(delegate {
                ChatUtil.SendSanitizedChatMessage($"/generalaction \"{action.Name.RawString}\"");
            });
        }

        public unsafe int GetIconId(uint actionId) { 
            // ERRATA - replace Materia Melding with Advanced Materia Melding if unlocked
            if (actionId == 12 && UIState.Instance()->Hotbar.IsActionUnlocked(12)) {
                actionId = 13;
            }
            
            return GetActionById(actionId)?.Icon ?? 0;
        }

        public unsafe List<ExecutableAction> GetAllowedItems() {
            List<ExecutableAction> actions = new(); 
            
            foreach (var action in ActionSheet) {
                if (this.GetIllegalActionIDs().Contains(action.RowId)) {
                    continue;
                }
                
                if (action.UnlockLink != 0 && !UIState.Instance()->Hotbar.IsActionUnlocked(action.UnlockLink)) {
                    continue;
                }
                
                actions.Add(GetExecutableAction(action));
            }

            return actions;
        }
    }
}
using System.Collections.Generic;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using Lumina.Excel.GeneratedSheets;
using XIVDeck.FFXIVPlugin.Base;

namespace XIVDeck.FFXIVPlugin.ActionExecutor.Strategies {
    public class GeneralActionStrategy : FixedCommandStrategy<GeneralAction> {
        protected override unsafe IEnumerable<uint> GetIllegalActionIDs() {
            var illegalActions = new List<uint> {
                13, // Advanced Materia Melding - automatically injected on use of Materia Melding
            };

            foreach (var action in Injections.DataManager.GetExcelSheet<GeneralAction>()!) {
                if (illegalActions.Contains(action.RowId)) continue;
                
                // empty or non-ui actions are considered illegal
                if (action.UIPriority == 0 || action.Name.RawString == "") {
                    illegalActions.Add(action.RowId);
                    continue;
                }

                // consider any locked actions illegal
                if (action.UnlockLink != 0 && !UIState.Instance()->Hotbar.IsActionUnlocked(action.UnlockLink)) {
                    illegalActions.Add(action.RowId);
                    continue;
                }
            }

            return illegalActions;
        }

        protected override string GetNameForAction(GeneralAction action) {
            return action.Name.RawString;
        }

        protected override HotbarSlotType GetHotbarSlotType() {
            return HotbarSlotType.GeneralAction;
        }

        protected override int GetIconForAction(GeneralAction action) {
            return action.Icon;
        }

        protected override unsafe string GetCommandToCallAction(GeneralAction action) {
            // ERRATA - replace Materia Melding with Advanced Materia Melding if unlocked
            if (action.RowId == 12 && UIState.Instance()->Hotbar.IsActionUnlocked(12)) {
                action = this.GetActionById(13);
            }
            
            return $"/generalaction \"{action.Name.RawString}\"";
        }
    }
}
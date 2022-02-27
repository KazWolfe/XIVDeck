using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using Lumina.Excel.GeneratedSheets;

namespace FFXIVPlugin.ActionExecutor.Strategies {
    public class GeneralActionStrategy : FixedCommandStrategy<GeneralAction> {
        protected override uint[] GetIllegalActionIDs() {
            return new uint[] {
                11, // empty
                13, // Advanced Materia Melding - automatically injected on use of Materia Melding
                18, // Set Down - can only be used in special content
                23, // Dismount - can only be used in special content
                24  // Flying Mount Roulette - does not exist in game UI
            };
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
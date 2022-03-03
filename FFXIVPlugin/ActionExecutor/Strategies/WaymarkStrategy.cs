using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using Lumina.Excel.GeneratedSheets;

namespace XIVDeck.FFXIVPlugin.ActionExecutor.Strategies {
    public class WaymarkStrategy : FixedCommandStrategy<FieldMarker> {
        protected override string GetNameForAction(FieldMarker action) {
            return action.Name.RawString;
        }

        protected override HotbarSlotType GetHotbarSlotType() {
            return HotbarSlotType.FieldMarker;
        }

        protected override int GetIconForAction(FieldMarker action) {
            return action.UiIcon;
        }

        protected override string GetCommandToCallAction(FieldMarker action) {
            return $"/waymark \"{action.Name.RawString}\"";
        }
    }
}
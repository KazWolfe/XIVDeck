using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using Lumina.Excel.GeneratedSheets;

namespace XIVDeck.FFXIVPlugin.ActionExecutor.Strategies {
    public class MarkerStrategy : FixedCommandStrategy<Marker> {
        protected override string GetNameForAction(Marker action) {
            return action.Name.RawString;
        }

        protected override HotbarSlotType GetHotbarSlotType() {
            return HotbarSlotType.Marker;
        }

        protected override int GetIconForAction(Marker action) {
            return action.Icon;
        }

        protected override string GetCommandToCallAction(Marker action) {
            return $"/marking \"{action.Name.RawString}\"";
        }
    }
}
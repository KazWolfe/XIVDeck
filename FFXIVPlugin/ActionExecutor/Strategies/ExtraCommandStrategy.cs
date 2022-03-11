using System;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using XIVDeck.FFXIVPlugin.Data.Sheets;

namespace XIVDeck.FFXIVPlugin.ActionExecutor.Strategies {
    public class ExtraCommandStrategy : FixedCommandStrategy<ExtraCommand> {
        protected override string GetNameForAction(ExtraCommand action) {
            return action.Name.RawString;
        }

        protected override HotbarSlotType GetHotbarSlotType() {
            return HotbarSlotType.FieldMarker;
        }

        protected override int GetIconForAction(ExtraCommand action) {
            return action.Icon;
        }

        protected override string GetCommandToCallAction(ExtraCommand action) {
            // ToDo: there has to be some better way to get this command.
            return action.RowId switch {
                1 => "/grouppose",
                2 => "/idlingcamera",
                3 => "/alarm",
                _ => throw new ArgumentException($"No command exists for Extra Command {action.Name.RawString}. REPORT THIS BUG!")
            };
        }
    }
}
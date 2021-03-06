using System;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using XIVDeck.FFXIVPlugin.Game.Sheets;
using XIVDeck.FFXIVPlugin.Resources.Localization;

namespace XIVDeck.FFXIVPlugin.ActionExecutor.Strategies; 

[ActionStrategy(HotbarSlotType.ExtraCommand)]
public class ExtraCommandStrategy : FixedCommandStrategy<ExtraCommand> {
    protected override string GetNameForAction(ExtraCommand action) {
        return action.Name.ToString();
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
            _ => throw new ArgumentException(string.Format(UIStrings.ExtraCommandStrategy_NoCommandError, action.Name))
        };
    }
}
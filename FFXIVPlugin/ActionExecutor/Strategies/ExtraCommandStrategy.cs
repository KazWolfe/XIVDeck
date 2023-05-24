using System;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using Lumina.Excel.GeneratedSheets;
using XIVDeck.FFXIVPlugin.Resources.Localization;

namespace XIVDeck.FFXIVPlugin.ActionExecutor.Strategies; 

[ActionStrategy(HotbarSlotType.ExtraCommand)]
public class ExtraCommandStrategy : FixedCommandStrategy<ExtraCommand> {
    protected override int GetIconForAction(ExtraCommand action) => action.Icon;
    
    protected override string GetCommandToCallAction(ExtraCommand action) {
        // ToDo: there has to be some better way to get this command.
        return action.RowId switch {
            1 => "/grouppose",
            2 => "/idlingcamera",
            3 => "/alarm",
            _ => throw new ArgumentException(string.Format(UIStrings.ExtraCommandStrategy_NoCommandError, action.Name))
        };
    }

    protected override ExecutableAction BuildExecutableAction(ExtraCommand action) {
        return new ExecutableAction {
            ActionId = (int) action.RowId,
            ActionName = action.Name.ToString(),
            IconId = this.GetIconForAction(action),
            HotbarSlotType = HotbarSlotType.ExtraCommand
        };
    }
}
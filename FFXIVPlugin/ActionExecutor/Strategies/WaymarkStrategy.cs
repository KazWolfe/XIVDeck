using System;
using Dalamud.Logging;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using Lumina.Excel.GeneratedSheets;
using XIVDeck.FFXIVPlugin.Base;
using XIVDeck.FFXIVPlugin.Game.Managers;

namespace XIVDeck.FFXIVPlugin.ActionExecutor.Strategies; 

[ActionStrategy(HotbarSlotType.FieldMarker)]
public class WaymarkStrategy : FixedCommandStrategy<FieldMarker> {
    protected override int GetIconForAction(FieldMarker action) => action.UiIcon;
    protected override string GetCommandToCallAction(FieldMarker action) => throw new NotSupportedException();
    
    protected override ExecutableAction BuildExecutableAction(FieldMarker action) {
        return new ExecutableAction {
            ActionId = (int) action.RowId,
            ActionName = action.Name.ToString(),
            IconId = this.GetIconForAction(action),
            HotbarSlotType = HotbarSlotType.FieldMarker
        };
    }

    protected override void ExecuteInner(FieldMarker action) {
        PluginLog.Debug($"Executing {action} ({action.Name}) directly via hotbar");
        
        Injections.Framework.RunOnFrameworkThread(delegate {
            HotbarManager.ExecuteHotbarAction(HotbarSlotType.FieldMarker, action.RowId);
        });
    }
}
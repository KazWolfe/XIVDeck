using System;
using Dalamud.Logging;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using Lumina.Excel.GeneratedSheets;
using XIVDeck.FFXIVPlugin.Base;
using XIVDeck.FFXIVPlugin.Game;
using XIVDeck.FFXIVPlugin.Game.Managers;

namespace XIVDeck.FFXIVPlugin.ActionExecutor.Strategies; 

[ActionStrategy(HotbarSlotType.FieldMarker)]
public class WaymarkStrategy : FixedCommandStrategy<FieldMarker> {
    protected override string GetNameForAction(FieldMarker action) => action.Name.ToString();

    protected override HotbarSlotType GetHotbarSlotType() => HotbarSlotType.FieldMarker;

    protected override int GetIconForAction(FieldMarker action) => action.UiIcon;

    protected override string GetCommandToCallAction(FieldMarker action) => throw new NotSupportedException();
    
    protected override void ExecuteInner(FieldMarker action) {
        PluginLog.Debug($"Executing {action} ({action.Name}) directly via hotbar");
        
        Injections.Framework.RunOnFrameworkThread(delegate {
            HotbarManager.ExecuteHotbarAction(HotbarSlotType.FieldMarker, action.RowId);
        });
    }
}
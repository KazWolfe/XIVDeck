using System;
using Dalamud.Logging;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using Lumina.Excel.GeneratedSheets;
using XIVDeck.FFXIVPlugin.Base;
using XIVDeck.FFXIVPlugin.Game;

namespace XIVDeck.FFXIVPlugin.ActionExecutor.Strategies; 

[ActionStrategy(HotbarSlotType.Marker)]
public class MarkerStrategy : FixedCommandStrategy<Marker> {
    protected override string GetNameForAction(Marker action) => action.Name.ToString();

    protected override HotbarSlotType GetHotbarSlotType() => HotbarSlotType.Marker;

    protected override int GetIconForAction(Marker action) => action.Icon;

    protected override string GetCommandToCallAction(Marker action) => throw new NotSupportedException();

    protected override void ExecuteInner(Marker action) {
        PluginLog.Debug($"Executing {action} ({action.Name}) directly via hotbar");
        
        Injections.Framework.RunOnFrameworkThread(delegate {
            GameUtils.ExecuteHotbarAction(HotbarSlotType.Marker, action.RowId);
        });
    }
}
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using XIVDeck.FFXIVPlugin.Base;
using XIVDeck.FFXIVPlugin.Game.Managers;
using Marker = XIVDeck.FFXIVPlugin.Game.Data.Marker;

namespace XIVDeck.FFXIVPlugin.ActionExecutor.Strategies; 

[ActionStrategy(HotbarSlotType.Marker)]
public class MarkerStrategy : FixedCommandStrategy<Marker> {
    
    protected override int GetIconForAction(Marker action) => action.Icon;
    
    protected override ExecutableAction BuildExecutableAction(Marker action) {
        return new ExecutableAction {
            ActionId = (int) action.RowId,
            ActionName = action.Name.ToString(),
            IconId = this.GetIconForAction(action),
            HotbarSlotType = HotbarSlotType.Marker,
            SortOrder = action.SortOrder,
        };
    }

    protected override void ExecuteInner(Marker action) {
        Injections.PluginLog.Debug($"Executing {action} ({action.Name}) directly via hotbar");
        
        Injections.Framework.RunOnFrameworkThread(delegate {
            HotbarManager.ExecuteHotbarAction(HotbarSlotType.Marker, action.RowId);
        });
    }
}
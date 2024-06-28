using static FFXIVClientStructs.FFXIV.Client.UI.Misc.RaptureHotbarModule;
using Lumina.Excel.GeneratedSheets;
using XIVDeck.FFXIVPlugin.Base;
using XIVDeck.FFXIVPlugin.Game.Managers;

namespace XIVDeck.FFXIVPlugin.ActionExecutor.Strategies;

[ActionStrategy(HotbarSlotType.ExtraCommand)]
public class ExtraCommandStrategy : FixedCommandStrategy<ExtraCommand> {
    protected override int GetIconForAction(ExtraCommand action) => action.Icon;

    protected override ExecutableAction BuildExecutableAction(ExtraCommand action) {
        return new ExecutableAction {
            ActionId = (int) action.RowId,
            ActionName = action.Name.ToString(),
            IconId = this.GetIconForAction(action),
            HotbarSlotType = HotbarSlotType.ExtraCommand
        };
    }

    protected override void ExecuteInner(ExtraCommand action) {
        Injections.PluginLog.Debug($"Executing hotbar slot: ExtraCommand#{action.RowId} ({action.Name})");
        Injections.Framework.RunOnFrameworkThread(delegate {
            HotbarManager.ExecuteHotbarAction(HotbarSlotType.ExtraCommand, action.RowId);
        });
    }
}

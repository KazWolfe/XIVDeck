using System.Threading.Tasks;
using Dalamud.Plugin.Services;
using Lumina.Excel.Sheets;
using Serilog;
using static FFXIVClientStructs.FFXIV.Client.UI.Misc.RaptureHotbarModule;
using XIVDeck.FFXIVPlugin.Game;
using XIVDeck.FFXIVPlugin.Game.Managers;
using XIVDeck.FFXIVPlugin.Contract;

namespace XIVDeck.FFXIVPlugin.ActionExecutor.Strategies;

[ActionStrategy(HotbarSlotType.ExtraCommand)]
public class ExtraCommandStrategy(IDataManager dataManager, ILogger pluginLog, IFramework framework,
    ActionAppearanceResolver appearanceResolver)
    : FixedCommandStrategy<ExtraCommand>(dataManager, appearanceResolver) {

    protected override ActionEntry BuildExecutableAction(ExtraCommand action) {
        return new ActionEntry {
            Id = (int) action.RowId,
            Name = action.Name.ToString(),
            Type = HotbarSlotType.ExtraCommand
        };
    }

    protected override async Task ExecuteInner(ExtraCommand action) {
        pluginLog.Debug("Executing ExtraCommand#{ActionId} ({ActionName})", action.RowId, action.Name.ExtractText());
        await framework.RunOnFrameworkThread(delegate {
            HotbarManager.ExecuteHotbarAction(HotbarSlotType.ExtraCommand, action.RowId);
        });
    }
}

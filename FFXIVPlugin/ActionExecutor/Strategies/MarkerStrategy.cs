using System.Threading.Tasks;
using Dalamud.Plugin.Services;
using Lumina.Excel.Sheets;
using Serilog;
using static FFXIVClientStructs.FFXIV.Client.UI.Misc.RaptureHotbarModule;
using XIVDeck.FFXIVPlugin.Game;
using XIVDeck.FFXIVPlugin.Game.Managers;
using XIVDeck.FFXIVPlugin.Contract;

namespace XIVDeck.FFXIVPlugin.ActionExecutor.Strategies;

[ActionStrategy(HotbarSlotType.Marker)]
public class MarkerStrategy(IDataManager dataManager, IFramework framework, ILogger log,
    ActionAppearanceResolver appearanceResolver) : FixedCommandStrategy<Marker>(dataManager, appearanceResolver) {

    protected override ActionEntry BuildExecutableAction(Marker action) {
        return new ActionEntry {
            Id = (int) action.RowId,
            Name = action.Name.ToString(),
            Type = HotbarSlotType.Marker,
            SortOrder = action.SortOrder,
        };
    }

    protected override async Task ExecuteInner(Marker action) {
        log.Debug("Executing Marker#{ActionId} ({ActionName}) directly via hotbar", action.RowId, action.Name.ExtractText());

        await framework.RunOnFrameworkThread(delegate {
            HotbarManager.ExecuteHotbarAction(HotbarSlotType.Marker, action.RowId);
        });
    }
}

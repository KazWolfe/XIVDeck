using System.Threading.Tasks;
using Dalamud.Plugin.Services;
using static FFXIVClientStructs.FFXIV.Client.UI.Misc.RaptureHotbarModule;
using Lumina.Excel.Sheets;
using Serilog;
using XIVDeck.FFXIVPlugin.Game;
using XIVDeck.FFXIVPlugin.Game.Managers;
using XIVDeck.FFXIVPlugin.Contract;

namespace XIVDeck.FFXIVPlugin.ActionExecutor.Strategies;

[ActionStrategy(HotbarSlotType.FieldMarker)]
public class WaymarkStrategy(IDataManager dataManager, IFramework framework, ILogger log,
    ActionAppearanceResolver appearanceResolver) :
    FixedCommandStrategy<FieldMarker>(dataManager, appearanceResolver) {

    protected override ActionEntry BuildExecutableAction(FieldMarker action) {
        return new ActionEntry {
            Id = (int)action.RowId,
            Name = action.Name.ToString(),
            Type = HotbarSlotType.FieldMarker
        };
    }

    protected override async Task ExecuteInner(FieldMarker action) {
        log.Debug("Executing FieldMarker#{ActionId} ({ActionName}) directly via hotbar", action.RowId, action.Name.ExtractText());

        await framework.RunOnFrameworkThread(delegate {
            HotbarManager.ExecuteHotbarAction(HotbarSlotType.FieldMarker, action.RowId);
        });
    }
}

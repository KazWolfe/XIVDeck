using Dalamud.Plugin.Services;
using Lumina.Excel.Sheets;
using Serilog;
using XIVDeck.FFXIVPlugin.Resources.Localization;
using static FFXIVClientStructs.FFXIV.Client.UI.Misc.RaptureHotbarModule;
using XIVDeck.FFXIVPlugin.Game;
using XIVDeck.FFXIVPlugin.Contract;

namespace XIVDeck.FFXIVPlugin.ActionExecutor.Strategies;

[ActionStrategy(HotbarSlotType.McGuffin)]
public class CollectionStrategy(IDataManager dataManager, ILogger pluginLog, IFramework framework, IUnlockState unlockState,
    ActionAppearanceResolver appearanceResolver)
    : UnlockableActionStrategy<McGuffin>(dataManager, framework, pluginLog, appearanceResolver) {
    protected override ActionEntry BuildActionEntry(McGuffin mcguffin) {
        var uiData = mcguffin.UIData.Value;

        return new ActionEntry {
            Id = (int) mcguffin.RowId,
            Name = uiData.Name.ToString(),
            Type = HotbarSlotType.McGuffin,
            SortOrder = uiData.Order
        };
    }

    protected override bool IsUnlocked(McGuffin mcguffin) => unlockState.IsMcGuffinUnlocked(mcguffin);

    protected override string GetNotFoundMessage(uint actionId) =>
        string.Format(UIStrings.CollectionStrategy_CollectionNotFoundError, actionId);
}

using Dalamud.Plugin.Services;
using Lumina.Excel.Sheets;
using Serilog;
using static FFXIVClientStructs.FFXIV.Client.UI.Misc.RaptureHotbarModule;
using XIVDeck.FFXIVPlugin.Game;
using XIVDeck.FFXIVPlugin.Contract;

namespace XIVDeck.FFXIVPlugin.ActionExecutor.Strategies;

[ActionStrategy(HotbarSlotType.Glasses)]
public class GlassesStrategy(IDataManager dataManager, ILogger pluginLog, IFramework framework, IUnlockState unlockState,
    ActionAppearanceResolver appearanceResolver)
    : UnlockableActionStrategy<Glasses>(dataManager, framework, pluginLog, appearanceResolver) {
    protected override ActionEntry BuildActionEntry(Glasses glasses) {
        return new ActionEntry {
            Id = (int) glasses.RowId,
            Name = glasses.Name.ToString(),
            Type = HotbarSlotType.Glasses,
            Category = glasses.Style.ValueNullable?.Name.ToString() ?? "Unknown",
            SortOrder = (int)(((glasses.Style.ValueNullable?.Order ?? 0) << (sizeof(ushort) * 8)) + glasses.RowId),
        };
    }

    protected override bool IsUnlocked(Glasses glasses) => unlockState.IsGlassesUnlocked(glasses);
}

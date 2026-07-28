using Dalamud.Plugin.Services;
using Lumina.Excel.Sheets;
using Serilog;
using static FFXIVClientStructs.FFXIV.Client.UI.Misc.RaptureHotbarModule;
using XIVDeck.FFXIVPlugin.Resources.Localization;
using XIVDeck.FFXIVPlugin.Game;
using XIVDeck.FFXIVPlugin.Contract;

namespace XIVDeck.FFXIVPlugin.ActionExecutor.Strategies;

[ActionStrategy(HotbarSlotType.Ornament)]
public class OrnamentStrategy(IDataManager dataManager, ILogger pluginLog, IFramework framework, IUnlockState unlockState,
    ActionAppearanceResolver appearanceResolver)
    : UnlockableActionStrategy<Ornament>(dataManager, framework, pluginLog, appearanceResolver) {
    protected override ActionEntry BuildActionEntry(Ornament ornament) {
        return new ActionEntry {
            Id = (int) ornament.RowId,
            Name = ornament.Singular.ToString(),
            Type = HotbarSlotType.Ornament,
            SortOrder = ornament.Order
        };
    }

    protected override bool IsUnlocked(Ornament ornament) => unlockState.IsOrnamentUnlocked(ornament);

    protected override string GetLockedMessage(Ornament ornament) =>
        string.Format(UIStrings.OrnamentStrategy_OrnamentLockedError, ornament.Singular);
}

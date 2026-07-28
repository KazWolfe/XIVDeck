using Dalamud.Plugin.Services;
using Lumina.Excel.Sheets;
using Serilog;
using static FFXIVClientStructs.FFXIV.Client.UI.Misc.RaptureHotbarModule;
using XIVDeck.FFXIVPlugin.Resources.Localization;
using XIVDeck.FFXIVPlugin.Utils;
using XIVDeck.FFXIVPlugin.Game;
using XIVDeck.FFXIVPlugin.Contract;

namespace XIVDeck.FFXIVPlugin.ActionExecutor.Strategies;

[ActionStrategy(HotbarSlotType.Companion)]
public class MinionStrategy(IDataManager dataManager, ILogger pluginLog, IFramework framework, IUnlockState unlockState,
    ActionAppearanceResolver appearanceResolver)
    : UnlockableActionStrategy<Companion>(dataManager, framework, pluginLog, appearanceResolver) {
    protected override ActionEntry BuildActionEntry(Companion minion) {
        return new ActionEntry {
            Id = (int) minion.RowId,
            Name = minion.Singular.ToString(),
            Type = HotbarSlotType.Companion,
            SortOrder = minion.Order
        };
    }

    protected override bool IsUnlocked(Companion minion) => unlockState.IsCompanionUnlocked(minion);

    protected override string GetNotFoundMessage(uint actionId) =>
        string.Format(UIStrings.MinionStrategy_MinionNotFoundError, actionId);

    protected override string GetLockedMessage(Companion minion) =>
        string.Format(UIStrings.MinionStrategy_MinionLockedError, minion.Singular.ToTitleCase());
}

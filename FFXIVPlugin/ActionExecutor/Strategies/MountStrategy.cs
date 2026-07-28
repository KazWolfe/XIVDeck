using Dalamud.Plugin.Services;
using Lumina.Excel.Sheets;
using Serilog;
using static FFXIVClientStructs.FFXIV.Client.UI.Misc.RaptureHotbarModule;
using XIVDeck.FFXIVPlugin.Resources.Localization;
using XIVDeck.FFXIVPlugin.Utils;
using XIVDeck.FFXIVPlugin.Game;
using XIVDeck.FFXIVPlugin.Contract;

namespace XIVDeck.FFXIVPlugin.ActionExecutor.Strategies;

[ActionStrategy(HotbarSlotType.Mount)]
public class MountStrategy(IDataManager dataManager, ILogger pluginLog, IFramework framework, IUnlockState unlockState,
    ActionAppearanceResolver appearanceResolver)
    : UnlockableActionStrategy<Mount>(dataManager, framework, pluginLog, appearanceResolver) {
    protected override ActionEntry BuildActionEntry(Mount mount) {
        return new ActionEntry {
            Id = (int) mount.RowId,
            Name = mount.Singular.ToString(),
            Type = HotbarSlotType.Mount,
            SortOrder = (mount.UIPriority <<  (sizeof(ushort) * 8)) + mount.Order
        };
    }

    protected override bool IsUnlocked(Mount mount) => unlockState.IsMountUnlocked(mount);

    protected override string GetNotFoundMessage(uint actionId) =>
        string.Format(UIStrings.MountStrategy_MountNotFoundError, actionId);

    protected override string GetLockedMessage(Mount mount) =>
        string.Format(UIStrings.MountStrategy_MountLockedError, mount.Singular.ToTitleCase());
}

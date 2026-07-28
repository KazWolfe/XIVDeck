using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using static FFXIVClientStructs.FFXIV.Client.UI.Misc.RaptureHotbarModule;
using Lumina.Excel.Sheets;
using Serilog;
using XIVDeck.FFXIVPlugin.ActionExecutor.Payloads;
using XIVDeck.FFXIVPlugin.Resources.Localization;
using XIVDeck.FFXIVPlugin.Game;
using XIVDeck.FFXIVPlugin.Contract;

namespace XIVDeck.FFXIVPlugin.ActionExecutor.Strategies;

[ActionStrategy(HotbarSlotType.MainCommand)]
public class MainCommandStrategy(IDataManager dataManager, ILogger pluginLog, IFramework framework,
    ActionAppearanceResolver appearanceResolver)
    : UnlockableActionStrategy<MainCommand>(dataManager, framework, pluginLog, appearanceResolver) {
    protected override ActionEntry BuildActionEntry(MainCommand mainCommand) {
        return new ActionEntry {
            Id = (int) mainCommand.RowId,
            Name = mainCommand.Name.ExtractText(),
            Category = mainCommand.MainCommandCategory.Value.Name.ExtractText(),
            Type = HotbarSlotType.MainCommand
        };
    }

    protected override unsafe bool IsUnlocked(MainCommand mainCommand) {
        return mainCommand.Category != 0 &&
               Framework.Instance()->GetUIModule()->IsMainCommandUnlocked(mainCommand.RowId);
    }

    protected override string? GetInvalidReason(MainCommand mainCommand) {
        return mainCommand.Category == 0
            ? string.Format(UIStrings.MainCommandStrategy_ActionInvalidError, mainCommand.RowId)
            : null;
    }

    protected override string GetLockedMessage(MainCommand mainCommand) {
        return string.Format(UIStrings.MainCommandStrategy_MainCommandLocked, mainCommand.Name);
    }

    protected override unsafe void ExecuteOnFramework(MainCommand mainCommand, ActionPayload? payload) {
        Framework.Instance()->GetUIModule()->ExecuteMainCommand(mainCommand.RowId);
    }
}

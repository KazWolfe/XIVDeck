using System;
using System.Collections.Generic;
using System.Linq;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using Lumina.Excel;
using Lumina.Excel.GeneratedSheets;
using XIVDeck.FFXIVPlugin.Base;
using XIVDeck.FFXIVPlugin.Exceptions;
using XIVDeck.FFXIVPlugin.Game;
using XIVDeck.FFXIVPlugin.Resources.Localization;

namespace XIVDeck.FFXIVPlugin.ActionExecutor.Strategies; 

[ActionStrategy(HotbarSlotType.MainCommand)]
public class MainCommandStrategy : IActionStrategy {
    private static readonly ExcelSheet<MainCommand> MainCommands = Injections.DataManager.GetExcelSheet<MainCommand>()!;
    private readonly UnlockHelper _unlockHelper = UnlockHelper.GetInstance();

    private static ExecutableAction GetExecutableAction(MainCommand mainCommand) {
        return new ExecutableAction {
            ActionId = (int) mainCommand.RowId,
            ActionName = mainCommand.Name.ToString(),
            IconId = mainCommand.Icon,
            Category = mainCommand.MainCommandCategory.Value!.Name.ToString(),
            HotbarSlotType = HotbarSlotType.MainCommand
        };
    }

    public unsafe void Execute(uint actionId, ActionPayload? _) {
        var mainCommand = MainCommands.GetRow(actionId);
        
        if (mainCommand == null || mainCommand.Category == 0)
            throw new InvalidOperationException(string.Format(UIStrings.MainCommandStrategy_ActionInvalidError, actionId));

        if (!this._unlockHelper.IsMainCommandUnlocked(actionId))
            throw new ActionLockedException(string.Format(UIStrings.MainCommandStrategy_MainCommandLocked, mainCommand.Name));

        Injections.Framework.RunOnFrameworkThread(delegate {
            Framework.Instance()->GetUiModule()->ExecuteMainCommand(actionId);
        });
    }

    public int GetIconId(uint actionId) {
        return MainCommands.GetRow(actionId)?.Icon ?? 0;
    }

    public List<ExecutableAction> GetAllowedItems() {
        return MainCommands
            .Where(r => r.Category != 0)
            .Where(r => this._unlockHelper.IsMainCommandUnlocked(r.RowId))
            .Select(GetExecutableAction)
            .ToList();
    }

    public ExecutableAction? GetExecutableActionById(uint actionId) {
        var action = Injections.DataManager.Excel.GetSheet<MainCommand>()!.GetRow(actionId);
        return action == null ? null : GetExecutableAction(action);
    }
        
}
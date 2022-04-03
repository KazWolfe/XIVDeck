using System;
using System.Collections.Generic;
using System.Linq;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using Lumina.Excel.GeneratedSheets;
using XIVDeck.FFXIVPlugin.Base;
using XIVDeck.FFXIVPlugin.Game;

namespace XIVDeck.FFXIVPlugin.ActionExecutor.Strategies; 

[ActionStrategy(HotbarSlotType.MainCommand)]
public class MainCommandStrategy : IActionStrategy {
    private readonly List<MainCommand> _mainCommandCache = new();

    private static ExecutableAction GetExecutableAction(MainCommand mainCommand) {
        return new ExecutableAction {
            ActionId = (int) mainCommand.RowId,
            ActionName = mainCommand.Name.ToString(),
            IconId = mainCommand.Icon,
            Category = mainCommand.MainCommandCategory.Value!.Name.ToString(),
            HotbarSlotType = HotbarSlotType.MainCommand
        };
    }
        
    public MainCommandStrategy() {
        // initialize the MainCommand cache with valid action types as part of construction.
        // this value is fixed so this is safe-ish.

        var sheet = Injections.DataManager.Excel.GetSheet<MainCommand>()!;

        foreach (var command in sheet) {
            // cheap but effective test; this ignores basically anything that's not in a menu
            if (command.Category == 0) continue;

            this._mainCommandCache.Add(command);
        }
    }

    public unsafe void Execute(uint actionId, dynamic? _) {
        if (this._mainCommandCache.All(command => actionId != command.RowId))
            throw new InvalidOperationException($"Main command action ID {actionId} is not valid.");
            
        TickScheduler.Schedule(delegate {
            Framework.Instance()->GetUiModule()->ExecuteMainCommand(actionId);
        });
    }

    public int GetIconId(uint actionId) {
        var action = Injections.DataManager.Excel.GetSheet<MainCommand>()!.GetRow(actionId);
        return action?.Icon ?? 0;
    }

    public List<ExecutableAction> GetAllowedItems() {
        return this._mainCommandCache.Select(GetExecutableAction).ToList();
    }

    public ExecutableAction? GetExecutableActionById(uint actionId) {
        var action = Injections.DataManager.Excel.GetSheet<MainCommand>()!.GetRow(actionId);
        return action == null ? null : GetExecutableAction(action);
    }
        
}
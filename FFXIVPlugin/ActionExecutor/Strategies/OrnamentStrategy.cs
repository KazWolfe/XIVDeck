using System.Collections.Generic;
using System.Linq;

using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using Lumina.Excel.GeneratedSheets;
using XIVDeck.FFXIVPlugin.Base;
using XIVDeck.FFXIVPlugin.Exceptions;
using XIVDeck.FFXIVPlugin.Game;
using XIVDeck.FFXIVPlugin.Game.Managers;
using XIVDeck.FFXIVPlugin.Resources.Localization;
using XIVDeck.FFXIVPlugin.Utils;

namespace XIVDeck.FFXIVPlugin.ActionExecutor.Strategies; 

[ActionStrategy(HotbarSlotType.Ornament)]
public class OrnamentStrategy : IActionStrategy {
    private static ExecutableAction GetExecutableAction(Ornament ornament) {
        return new ExecutableAction {
            ActionId = (int) ornament.RowId,
            ActionName = ornament.Singular.ToString(),
            IconId = ornament.Icon,
            HotbarSlotType = HotbarSlotType.Ornament,
            SortOrder = ornament.Order
        };
    }
        
    private static Ornament? GetOrnamentById(uint id) {
        return Injections.DataManager.Excel.GetSheet<Ornament>()!.GetRow(id);
    }
        
    public List<ExecutableAction> GetAllowedItems() {
        return Injections.DataManager.GetExcelSheet<Ornament>()!
            .Where(o => o.IsUnlocked())
            .Select(GetExecutableAction)
            .ToList();
    }

    public ExecutableAction? GetExecutableActionById(uint actionId) {
        var action = GetOrnamentById(actionId);
        return action == null ? null : GetExecutableAction(action);
    }

    public void Execute(uint actionId, ActionPayload? _) {
        var ornament = GetOrnamentById(actionId);

        if (ornament == null) {
            throw new ActionNotFoundException(HotbarSlotType.Ornament, actionId);
        }

        if (!ornament.IsUnlocked()) {
            throw new ActionLockedException(string.Format(UIStrings.OrnamentStrategy_OrnamentLockedError, ornament.Singular));
        }
            
        Injections.PluginLog.Debug($"Executing hotbar slot: Ornament#{ornament.RowId} ({ornament.Singular.ToTitleCase()})");
        Injections.Framework.RunOnFrameworkThread(delegate {
            HotbarManager.ExecuteHotbarAction(HotbarSlotType.Ornament, ornament.RowId);
        });
    }

    public int GetIconId(uint item) {
        return GetOrnamentById(item)?.Icon ?? 0;
    }
}
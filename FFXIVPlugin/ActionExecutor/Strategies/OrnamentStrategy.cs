using System.Collections.Generic;
using System.Linq;
using Dalamud.Logging;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using Lumina.Excel.GeneratedSheets;
using XIVDeck.FFXIVPlugin.Base;
using XIVDeck.FFXIVPlugin.Exceptions;
using XIVDeck.FFXIVPlugin.Game;
using XIVDeck.FFXIVPlugin.Game.Chat;
using XIVDeck.FFXIVPlugin.Resources.Localization;

namespace XIVDeck.FFXIVPlugin.ActionExecutor.Strategies; 

[ActionStrategy(HotbarSlotType.FashionAccessory)]
public class OrnamentStrategy : IActionStrategy {
    private static ExecutableAction GetExecutableAction(Ornament ornament) {
        return new ExecutableAction {
            ActionId = (int) ornament.RowId,
            ActionName = ornament.Singular.ToString(),
            IconId = ornament.Icon,
            HotbarSlotType = HotbarSlotType.FashionAccessory,
            SortOrder = ornament.Order
        };
    }
        
    private static Ornament? GetOrnamentById(uint id) {
        return Injections.DataManager.Excel.GetSheet<Ornament>()!.GetRow(id);
    }
        
    public List<ExecutableAction>? GetAllowedItems() {
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
            throw new ActionNotFoundException(HotbarSlotType.FashionAccessory, actionId);
        }

        if (!ornament.IsUnlocked()) {
            throw new ActionLockedException(string.Format(UIStrings.OrnamentStrategy_OrnamentLockedError, ornament.Singular));
        }
            
        var command = $"/fashion \"{ornament.Singular}\"";
        PluginLog.Debug($"Executing command: {command}");
        Injections.Framework.RunOnFrameworkThread(delegate {
            ChatHelper.GetInstance().SendSanitizedChatMessage(command);
        });
    }

    public int GetIconId(uint item) {
        return GetOrnamentById(item)?.Icon ?? 0;
    }
}
using System.Collections.Generic;
using System.Linq;
using static FFXIVClientStructs.FFXIV.Client.UI.Misc.RaptureHotbarModule;
using XIVDeck.FFXIVPlugin.Base;
using XIVDeck.FFXIVPlugin.Exceptions;
using XIVDeck.FFXIVPlugin.Game;
using XIVDeck.FFXIVPlugin.Game.Data;
using XIVDeck.FFXIVPlugin.Game.Managers;
using XIVDeck.FFXIVPlugin.Utils;

namespace XIVDeck.FFXIVPlugin.ActionExecutor.Strategies;

[ActionStrategy(HotbarSlotType.Glasses)]
public class GlassesStrategy : IActionStrategy {

    private static ExecutableAction GetExecutableAction(Glasses glasses) {
        return new ExecutableAction {
            ActionId = (int) glasses.RowId,
            ActionName = glasses.Name.ToString(),
            IconId = glasses.Icon,
            HotbarSlotType = HotbarSlotType.Glasses,
            Category = glasses.Style?.Value?.Name.ToString() ?? "Unknown",
            SortOrder = (int)(((glasses.Style?.Value?.Order ?? 0) << sizeof(ushort)) + glasses.RowId),
        };
    }

    private static Glasses? GetGlassesById(uint id) {
        return Injections.DataManager.Excel.GetSheet<Glasses>()?.GetRow(id);
    }

    public List<ExecutableAction> GetAllowedItems() {
        return Injections.DataManager.GetExcelSheet<Glasses>()!
            .Where(g => g.IsUnlocked())
            .Select(GetExecutableAction)
            .ToList();
    }

    public ExecutableAction? GetExecutableActionById(uint actionId) {
        var action = GetGlassesById(actionId);
        return action == null ? null : GetExecutableAction(action);
    }

    public void Execute(uint actionId, ActionPayload? _) {
        var glasses = GetGlassesById(actionId);

        if (glasses == null) {
            throw new ActionNotFoundException(HotbarSlotType.Glasses, actionId);
        }

        if (!glasses.IsUnlocked()) {
            throw new ActionLockedException($"The {glasses.Name} aren't unlocked and therefore can't be used");
        }

        Injections.PluginLog.Debug($"Executing hotbar slot: Glasses#{glasses.RowId} ({glasses.Name.ToTitleCase()})");
        Injections.Framework.RunOnFrameworkThread(delegate {
            HotbarManager.ExecuteHotbarAction(HotbarSlotType.Glasses, glasses.RowId);
        });
    }

    public int GetIconId(uint item) {
        return GetGlassesById(item)?.Icon ?? 0;
    }
}

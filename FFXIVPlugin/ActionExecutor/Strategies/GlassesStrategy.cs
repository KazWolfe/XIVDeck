using System.Collections.Generic;
using System.Linq;
using Lumina.Excel.Sheets;
using static FFXIVClientStructs.FFXIV.Client.UI.Misc.RaptureHotbarModule;
using XIVDeck.FFXIVPlugin.Base;
using XIVDeck.FFXIVPlugin.Exceptions;
using XIVDeck.FFXIVPlugin.Game;
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
            Category = glasses.Style.ValueNullable?.Name.ToString() ?? "Unknown",
            SortOrder = (int)(((glasses.Style.ValueNullable?.Order ?? 0) << sizeof(ushort)) + glasses.RowId),
        };
    }

    private static Glasses? GetGlassesById(uint id) {
        return Injections.DataManager.Excel.GetSheet<Glasses>().GetRowOrDefault(id);
    }

    public List<ExecutableAction> GetAllowedItems() {
        return Injections.DataManager.GetExcelSheet<Glasses>()
            .Where(g => g.IsUnlocked())
            .Select(GetExecutableAction)
            .ToList();
    }

    public ExecutableAction? GetExecutableActionById(uint actionId) {
        var action = GetGlassesById(actionId);
        return action == null ? null : GetExecutableAction(action.Value);
    }

    public void Execute(uint actionId, ActionPayload? _) {
        var glasses = GetGlassesById(actionId);

        if (glasses == null) {
            throw new ActionNotFoundException(HotbarSlotType.Glasses, actionId);
        }

        if (!glasses.Value.IsUnlocked()) {
            throw new ActionLockedException($"The {glasses.Value.Name} aren't unlocked and therefore can't be used");
        }

        Injections.PluginLog.Debug($"Executing hotbar slot: Glasses#{glasses.Value.RowId} ({glasses.Value.Name.ToTitleCase()})");
        Injections.Framework.RunOnFrameworkThread(delegate {
            HotbarManager.ExecuteHotbarAction(HotbarSlotType.Glasses, glasses.Value.RowId);
        });
    }

    public int GetIconId(uint item) {
        return GetGlassesById(item)?.Icon ?? 0;
    }
}

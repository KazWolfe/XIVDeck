using System;
using System.Collections.Generic;
using System.Linq;
using Lumina.Excel;
using Lumina.Excel.Sheets;
using XIVDeck.FFXIVPlugin.Base;
using XIVDeck.FFXIVPlugin.Game;
using XIVDeck.FFXIVPlugin.Game.Managers;
using XIVDeck.FFXIVPlugin.Resources.Localization;
using static FFXIVClientStructs.FFXIV.Client.UI.Misc.RaptureHotbarModule;

namespace XIVDeck.FFXIVPlugin.ActionExecutor.Strategies;

[ActionStrategy(HotbarSlotType.McGuffin)]
public class CollectionStrategy : IActionStrategy {
    private static readonly ExcelSheet<McGuffin> Sheet = Injections.DataManager.Excel.GetSheet<McGuffin>()!;

    private static ExecutableAction GetExecutableAction(McGuffin mcguffin) {
        var uiData = mcguffin.UIData.Value!;

        return new ExecutableAction {
            ActionId = (int) mcguffin.RowId,
            ActionName = uiData.Name.ToString(),
            IconId = (int) uiData.Icon,
            Category = null,
            HotbarSlotType = HotbarSlotType.McGuffin,
            SortOrder = uiData.Order
        };
    }

    private static McGuffin? GetMcGuffinById(uint id) {
        return Sheet.GetRow(id);
    }

    public ExecutableAction? GetExecutableActionById(uint actionId) {
        var mcguffin = GetMcGuffinById(actionId);

        return mcguffin == null ? null : GetExecutableAction(mcguffin.Value);
    }

    public List<ExecutableAction> GetAllowedItems() {
        return Sheet.Where(m => m.IsUnlocked())
            .Select(GetExecutableAction)
            .ToList();
    }

    public void Execute(uint actionId, ActionPayload? _) {
        var mcguffin = GetMcGuffinById(actionId);

        if (mcguffin == null) {
            throw new ArgumentOutOfRangeException(nameof(actionId), string.Format(UIStrings.CollectionStrategy_CollectionNotFoundError, actionId));
        }

        Injections.Framework.RunOnFrameworkThread(delegate {
            HotbarManager.ExecuteHotbarAction(HotbarSlotType.McGuffin, actionId);
        });
    }

    public int GetIconId(uint item) {
        return (int) (GetMcGuffinById(item)?.UIData.ValueNullable?.Icon ?? 0);

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using Lumina.Excel;
using XIVDeck.FFXIVPlugin.Base;
using XIVDeck.FFXIVPlugin.Game;
using XIVDeck.FFXIVPlugin.Game.Sheets;
using XIVDeck.FFXIVPlugin.Resources.Localization;

namespace XIVDeck.FFXIVPlugin.ActionExecutor.Strategies; 

[ActionStrategy(HotbarSlotType.Collection)]
public class CollectionStrategy : IActionStrategy {
    private static readonly GameStateCache GameStateCache = XIVDeckPlugin.Instance.GameStateCache;
    private static readonly ExcelSheet<McGuffin> Sheet = Injections.DataManager.Excel.GetSheet<McGuffin>()!;

    private static ExecutableAction GetExecutableAction(McGuffin mcguffin) {
        var uiData = mcguffin.UIData.Value!;
            
        return new ExecutableAction {
            ActionId = (int) mcguffin.RowId, 
            ActionName = uiData.Name.ToString(), 
            IconId = (int) uiData.Icon,
            Category = null,
            HotbarSlotType = HotbarSlotType.Collection
        };
    }

    private static McGuffin? GetMcGuffinById(uint id) {
        return Sheet.GetRow(id);
    }

    public ExecutableAction? GetExecutableActionById(uint actionId) {
        var mcguffin = GetMcGuffinById(actionId);

        return mcguffin == null ? null : GetExecutableAction(mcguffin);
    }

    public List<ExecutableAction> GetAllowedItems() {
        GameStateCache.Refresh();

        return Sheet.Where(m => m.IsUnlocked())
            .Select(GetExecutableAction)
            .ToList();
    }

    public void Execute(uint actionId, dynamic? _) {
        var mcguffin = GetMcGuffinById(actionId);

        if (mcguffin == null) {
            throw new ArgumentOutOfRangeException(nameof(actionId), string.Format(UIStrings.CollectionStrategy_CollectionNotFoundError, actionId));
        }
            
        Injections.Framework.RunOnFrameworkThread(delegate {
            GameUtils.ExecuteHotbarAction(HotbarSlotType.Collection, actionId);
        });
    }

    public int GetIconId(uint item) {
        return (int) (GetMcGuffinById(item)?.UIData.Value?.Icon ?? 0);

    }
}
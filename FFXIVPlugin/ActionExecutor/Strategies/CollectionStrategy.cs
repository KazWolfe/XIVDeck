using System;
using System.Collections.Generic;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using Lumina.Excel;
using XIVDeck.FFXIVPlugin.Base;
using XIVDeck.FFXIVPlugin.Game;
using XIVDeck.FFXIVPlugin.Game.Sheets;

namespace XIVDeck.FFXIVPlugin.ActionExecutor.Strategies; 

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

        var results = new List<ExecutableAction>();

        foreach (var mcguffin in Sheet) {
            if (GameStateCache.IsMcGuffinUnlocked(mcguffin.RowId)) {
                results.Add(GetExecutableAction(mcguffin));
            }
        }

        return results;
    }

    public void Execute(uint actionId, dynamic? _) {
        var mcguffin = GetMcGuffinById(actionId);

        if (mcguffin == null) {
            throw new ArgumentOutOfRangeException(nameof(actionId), $"No Collection with ID {actionId} exists.");
        }
            
        TickScheduler.Schedule(delegate {
            XIVDeckPlugin.Instance.SigHelper.ExecuteHotbarAction(HotbarSlotType.Collection, actionId);
        });
    }

    public int GetIconId(uint item) {
        return (int) (GetMcGuffinById(item)?.UIData.Value?.Icon ?? 0);

    }
}
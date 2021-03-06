using System;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Logging;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using Lumina.Excel;
using Lumina.Excel.GeneratedSheets;
using XIVDeck.FFXIVPlugin.Base;
using XIVDeck.FFXIVPlugin.Exceptions;
using XIVDeck.FFXIVPlugin.Game;
using XIVDeck.FFXIVPlugin.Resources.Localization;

namespace XIVDeck.FFXIVPlugin.ActionExecutor.Strategies; 

[ActionStrategy(HotbarSlotType.GeneralAction)]
public class GeneralActionStrategy : IActionStrategy {
    private List<uint> _illegalActionCache = new();
    private static readonly ExcelSheet<GeneralAction> ActionSheet = Injections.DataManager.Excel.GetSheet<GeneralAction>()!;

    private static ExecutableAction GetExecutableAction(GeneralAction action) {
        return new ExecutableAction {
            ActionId = (int) action.RowId,
            ActionName = action.Name.ToString(),
            IconId = action.Icon,
            HotbarSlotType = HotbarSlotType.GeneralAction
        };
    }
        
    private static GeneralAction? GetActionById(uint actionId) {
        return ActionSheet.GetRow(actionId);
    }

    private IEnumerable<uint> GetIllegalActionIDs() {
        if (this._illegalActionCache.Count > 0) {
            return this._illegalActionCache;
        }
            
        var illegalActions = new List<uint> {
            13, // Advanced Materia Melding - automatically injected on use of Materia Melding
            29, // Sort Pet Hotbar (Normal) - contextual
            30, // Sort Pet Hotbar (Cross)  - contextual
        };

        foreach (var action in ActionSheet) {
            if (illegalActions.Contains(action.RowId)) continue;
                
            // empty or non-ui actions are considered illegal
            if (action.UIPriority == 0 || action.Name.ToString() == "") {
                illegalActions.Add(action.RowId);
            }
        }

        this._illegalActionCache = illegalActions;
        return illegalActions;
    }

    public unsafe ExecutableAction? GetExecutableActionById(uint actionId) {
        var action = GetActionById(actionId);

        if (action == null) return null;
            
        // ERRATA - swap out melding
        if (actionId == 12 && UIState.Instance()->IsUnlockLinkUnlocked(12)) {
            action = GetActionById(13)!;
        }

        return GetExecutableAction(action);
    }

    public unsafe void Execute(uint actionId, dynamic? _) {
        var action = GetActionById(actionId);
            
        if (action == null) {
            throw new ActionNotFoundException(HotbarSlotType.GeneralAction, actionId);
        }
            
        if (this.GetIllegalActionIDs().Contains(actionId)) {
            throw new ArgumentOutOfRangeException(nameof(actionId),
                string.Format(UIStrings.GeneralActionStrategy_ActionIllegalError, action.Name, actionId));
        }
            
        if (action.UnlockLink != 0 && !UIState.Instance()->IsUnlockLinkUnlocked(action.UnlockLink)) {
            throw new ActionLockedException(string.Format(UIStrings.GeneralActionStrategy_ActionLockedError, action.Name));
        }
            
        if (actionId == 12 && UIState.Instance()->IsUnlockLinkUnlocked(12)) {
            action = GetActionById(13)!;
        }

        var command = $"/generalaction \"{action.Name}\"";
            
        PluginLog.Debug($"Would execute command: {command}");
        Injections.Framework.RunOnFrameworkThread(delegate { GameUtils.SendSanitizedChatMessage(command); });
    }

    public unsafe int GetIconId(uint actionId) { 
        // ERRATA - replace Materia Melding with Advanced Materia Melding if unlocked
        if (actionId == 12 && UIState.Instance()->IsUnlockLinkUnlocked(12)) {
            actionId = 13;
        }
            
        return GetActionById(actionId)?.Icon ?? 0;
    }

    public unsafe List<ExecutableAction> GetAllowedItems() {
        List<ExecutableAction> actions = new(); 
            
        foreach (var action in ActionSheet) {
            if (this.GetIllegalActionIDs().Contains(action.RowId)) {
                continue;
            }
                
            if (action.UnlockLink != 0 && !UIState.Instance()->IsUnlockLinkUnlocked(action.UnlockLink)) {
                continue;
            }
                
            actions.Add(GetExecutableAction(action));
        }

        return actions;
    }
}
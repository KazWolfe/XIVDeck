using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dalamud.Plugin.Services;
using Dalamud.Utility;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using static FFXIVClientStructs.FFXIV.Client.UI.Misc.RaptureHotbarModule;
using Lumina.Excel;
using Lumina.Excel.Sheets;
using Serilog;
using XIVDeck.FFXIVPlugin.ActionExecutor.Payloads;
using XIVDeck.FFXIVPlugin.Exceptions;
using XIVDeck.FFXIVPlugin.Game.Managers;
using XIVDeck.FFXIVPlugin.Resources.Localization;
using XIVDeck.FFXIVPlugin.Game;
using ActionAppearance = XIVDeck.FFXIVPlugin.Contract.ActionAppearance;
using XIVDeck.FFXIVPlugin.Contract;

namespace XIVDeck.FFXIVPlugin.ActionExecutor.Strategies;

[ActionStrategy(HotbarSlotType.GeneralAction)]
public class GeneralActionStrategy(
    IDataManager dataManager,
    ILogger pluginLog,
    IFramework framework,
    ActionAppearanceResolver appearanceResolver) : IActionStrategy {
    private List<uint> _illegalActionCache = [];

    private readonly ExcelSheet<GeneralAction> _actionSheet = dataManager.Excel.GetSheet<GeneralAction>();

    private static ActionEntry GetExecutableAction(GeneralAction action) {
        return new ActionEntry {
            Id = (int)action.RowId,
            Name = action.Name.ToString(),
            Type = HotbarSlotType.GeneralAction,
            SortOrder = action.UIPriority,
        };
    }

    private GeneralAction? GetActionById(uint actionId) {
        return this._actionSheet.GetRowOrDefault(actionId);
    }

    private IEnumerable<uint> GetIllegalActionIDs() {
        if (this._illegalActionCache.Count > 0) {
            return this._illegalActionCache;
        }

        var illegalActions = this._actionSheet
            .Where(action => action.UIPriority == 0 || action.Name.ExtractText().IsNullOrEmpty())
            .Select(a => a.RowId)
            .ToList();

        illegalActions.AddRange([
            13, // automatically substituted for Materia Melding by our client
            29, // Sort Pet Hotbar (Normal) - contextual
            30 // Sort Pet Hotbar (Cross)  - contextual
        ]);

        this._illegalActionCache = illegalActions;
        return illegalActions;
    }

    private static unsafe bool IsUnlockLinkUnlocked(uint linkId) => UIState.Instance()->IsUnlockLinkUnlocked(linkId);

    /// <summary>
    /// Resolve action IDs that may be changed or substituted via game behavior.
    /// </summary>
    /// <param name="actionId">The action ID to evaluate.</param>
    /// <returns>The substituted action ID.</returns>
    private static uint SubstituteActionId(uint actionId) {
        // Replaces Materia Melding with Advanced Materia Melding, if enabled.
        if (actionId == 12 && IsUnlockLinkUnlocked(12)) {
            return 13;
        }

        return actionId;
    }

    public ActionEntry? GetActionEntryById(uint actionId) {
        var action = this.GetActionById(SubstituteActionId(actionId));

        return action == null ? null : GetExecutableAction(action.Value);
    }

    public async Task Execute(uint actionId, ActionPayload? _) {
        var action = this.GetActionById(actionId);

        if (action == null) {
            throw new ActionNotFoundException(HotbarSlotType.GeneralAction, actionId);
        }

        if (this.GetIllegalActionIDs().Contains(actionId)) {
            throw new ActionInvalidException(string.Format(UIStrings.GeneralActionStrategy_ActionIllegalError,
                action.Value.Name, actionId));
        }

        if (action.Value.UnlockLink != 0 && !IsUnlockLinkUnlocked(action.Value.UnlockLink)) {
            throw new ActionLockedException(string.Format(UIStrings.GeneralActionStrategy_ActionLockedError,
                action.Value.Name));
        }

        action = this.GetActionById(SubstituteActionId(actionId))!;

        pluginLog.Debug("Executing GeneralAction#{ActionId} ({ActionName})", action.Value.RowId,
            action.Value.Name.ExtractText());
        await framework.RunOnFrameworkThread(delegate {
            HotbarManager.ExecuteHotbarAction(HotbarSlotType.GeneralAction, action.Value.RowId);
        });
    }

    public List<ActionEntry> GetSelectableActions() {
        return this._actionSheet.Where(action => !this.GetIllegalActionIDs().Contains(action.RowId))
            .Where(action => action.UnlockLink == 0 || IsUnlockLinkUnlocked(action.UnlockLink))
            .Select(GetExecutableAction).ToList();
    }

    public Task<ActionAppearance> GetAppearance(uint actionId) {
        return appearanceResolver.GetActionAppearance(HotbarSlotType.GeneralAction, SubstituteActionId(actionId));
    }
}

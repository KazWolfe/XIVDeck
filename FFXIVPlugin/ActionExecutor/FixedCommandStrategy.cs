using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Dalamud.Plugin.Services;
using Lumina.Excel;
using XIVDeck.FFXIVPlugin.ActionExecutor.Payloads;
using XIVDeck.FFXIVPlugin.Exceptions;
using XIVDeck.FFXIVPlugin.Game;
using XIVDeck.FFXIVPlugin.Resources.Localization;
using ActionAppearance = XIVDeck.FFXIVPlugin.Contract.ActionAppearance;
using HotbarSlotType = FFXIVClientStructs.FFXIV.Client.UI.Misc.RaptureHotbarModule.HotbarSlotType;
using XIVDeck.FFXIVPlugin.Contract;

namespace XIVDeck.FFXIVPlugin.ActionExecutor;

public abstract class FixedCommandStrategy<T> : IActionStrategy where T : struct, IExcelRow<T> {
    private readonly List<ActionEntry> _actionCache = [];

    private readonly IDataManager _dataManager;
    private readonly ActionAppearanceResolver _appearanceResolver;

    protected HotbarSlotType SlotType { get; }

    protected FixedCommandStrategy(IDataManager dataManager, ActionAppearanceResolver appearanceResolver) {
        this._dataManager = dataManager;
        this._appearanceResolver = appearanceResolver;

        this.SlotType = this.GetType().GetCustomAttribute<ActionStrategyAttribute>()?.HotbarSlotType ??
                        throw new InvalidOperationException($"{this.GetType().Name} is missing [ActionStrategy].");
    }

    protected abstract ActionEntry? BuildExecutableAction(T action);

    protected virtual IEnumerable<uint> GetIllegalActionIDs() => Array.Empty<uint>();

    private T? GetActionById(uint id) {
        // should never be null, T is inherently handled by Lumina
        return this._dataManager.Excel.GetSheet<T>().GetRowOrDefault(id);
    }

    public List<ActionEntry> GetSelectableActions() {
        if (this._actionCache.Count > 0) {
            // this is (relatively) safe as general actions shouldn't (can't) be added midway through the game.
            // so let's just cache them and return whenever this is called just to save a tiiiny amount of runtime
            return this._actionCache;
        }

        var sheet = this._dataManager.Excel.GetSheet<T>();

        if (sheet == null) {
            throw new NullReferenceException(string.Format(UIStrings.FixedCommandStrategy_SheetNotFoundError, typeof(T).Name));
        }

        foreach (var row in sheet) {
            // skip illegal action IDs
            if (row.RowId == 0) continue;
            if (this.GetIllegalActionIDs().Contains(row.RowId)) continue;

            var action = this.BuildExecutableAction(row);

            if (action == null || string.IsNullOrEmpty(action.Name)) continue;

            this._actionCache.Add(action);
        }

        return this._actionCache;
    }

    public ActionEntry? GetActionEntryById(uint actionId) {
        return this.GetSelectableActions().Find(ac => ac.Id == actionId);
    }

    public async Task Execute(uint actionId, ActionPayload? _) {
        if (this.GetIllegalActionIDs().Contains(actionId))
            throw new ActionInvalidException(string.Format(UIStrings.FixedCommandStrategy_IllegalActionError, actionId));

        var action = this.GetActionById(actionId);

        if (action == null) {
            throw new ActionNotFoundException(
                string.Format(UIStrings.FixedCommandStrategy_ActionNotFoundError, typeof(T), actionId));
        }

        // shenanigans, but allows us to ignore the entire text command processing chain if necessary
        await this.ExecuteInner(action.Value);
    }

    protected abstract Task ExecuteInner(T action);

    public Task<ActionAppearance> GetAppearance(uint actionId) =>
        this._appearanceResolver.GetActionAppearance(this.SlotType, actionId);
}

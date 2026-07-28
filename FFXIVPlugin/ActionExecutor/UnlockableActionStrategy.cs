using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Dalamud.Plugin.Services;
using Lumina.Excel;
using Serilog;
using XIVDeck.FFXIVPlugin.ActionExecutor.Payloads;
using XIVDeck.FFXIVPlugin.Exceptions;
using XIVDeck.FFXIVPlugin.Game;
using XIVDeck.FFXIVPlugin.Game.Managers;
using XIVDeck.FFXIVPlugin.Resources.Localization;
using static FFXIVClientStructs.FFXIV.Client.UI.Misc.RaptureHotbarModule;
using ActionAppearance = XIVDeck.FFXIVPlugin.Contract.ActionAppearance;
using XIVDeck.FFXIVPlugin.Contract;

namespace XIVDeck.FFXIVPlugin.ActionExecutor;

/// <summary>
/// Base for strategies backed by a single Excel sheet whose rows are individually unlocked by the player (minions,
/// mounts, emotes, etc.).
/// </summary>
public abstract class UnlockableActionStrategy<T> : IActionStrategy where T : struct, IExcelRow<T> {
    private readonly IFramework _framework;
    private readonly ILogger _log;
    private readonly ActionAppearanceResolver _appearanceResolver;

    protected ExcelSheet<T> Sheet { get; }
    protected HotbarSlotType SlotType { get; }

    protected UnlockableActionStrategy(IDataManager dataManager, IFramework framework, ILogger log,
        ActionAppearanceResolver appearanceResolver) {
        this._framework = framework;
        this._log = log;
        this._appearanceResolver = appearanceResolver;

        this.Sheet = dataManager.Excel.GetSheet<T>();
        this.SlotType = this.GetType().GetCustomAttribute<ActionStrategyAttribute>()?.HotbarSlotType ??
                        throw new InvalidOperationException($"{this.GetType().Name} is missing [ActionStrategy].");
    }

    protected abstract ActionEntry BuildActionEntry(T row);

    protected abstract bool IsUnlocked(T row);

    protected virtual string GetNotFoundMessage(uint actionId) =>
        string.Format(UIStrings.ActionNotFoundException_Message, this.SlotType, actionId);

    /// <summary>
    /// Why <paramref name="row"/> can never be executed, whatever the player has unlocked, or <c>null</c> if it can.
    /// Checked before <see cref="IsUnlocked"/>, so such rows fail as invalid rather than locked.
    /// </summary>
    protected virtual string? GetInvalidReason(T row) => null;

    protected virtual string GetLockedMessage(T row) =>
        string.Format(UIStrings.ActionLockedException_Message, this.SlotType, row.RowId);

    /// <summary>
    /// Performs the actual execution. Always called on the framework thread, after lookup and unlock checks pass.
    /// </summary>
    protected virtual void ExecuteOnFramework(T row, ActionPayload? payload) {
        HotbarManager.ExecuteHotbarAction(this.SlotType, row.RowId);
    }

    public virtual Type? GetPayloadType() => null;

    public virtual Task<ActionAppearance> GetAppearance(uint actionId) =>
        this._appearanceResolver.GetActionAppearance(this.SlotType, actionId);

    public ActionEntry? GetActionEntryById(uint actionId) {
        var row = this.Sheet.GetRowOrDefault(actionId);
        return row == null ? null : this.BuildActionEntry(row.Value);
    }

    public List<ActionEntry> GetSelectableActions() {
        return [.. this.Sheet.Where(this.IsUnlocked).Select(this.BuildActionEntry)];
    }

    public async Task Execute(uint actionId, ActionPayload? payload = null) {
        var row = this.Sheet.GetRowOrDefault(actionId) ??
                  throw new ActionNotFoundException(this.GetNotFoundMessage(actionId));

        if (this.GetInvalidReason(row) is { } invalidReason) {
            throw new ActionInvalidException(invalidReason);
        }

        if (!this.IsUnlocked(row)) {
            throw new ActionLockedException(this.GetLockedMessage(row));
        }

        this._log.Debug("Executing {SlotType}#{ActionId}", this.SlotType, actionId);
        await this._framework.RunOnFrameworkThread(() => this.ExecuteOnFramework(row, payload));
    }
}

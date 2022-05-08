using System;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Logging;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using Lumina.Excel;
using XIVDeck.FFXIVPlugin.Base;
using XIVDeck.FFXIVPlugin.Game;
using XIVDeck.FFXIVPlugin.Resources.Localization;
using XIVDeck.FFXIVPlugin.Utils;

namespace XIVDeck.FFXIVPlugin.ActionExecutor;

public abstract class FixedCommandStrategy<T> : IActionStrategy where T : ExcelRow {
    private readonly List<ExecutableAction> _actionCache = new();

    protected abstract string GetNameForAction(T action);
    protected abstract HotbarSlotType GetHotbarSlotType();
    protected abstract int GetIconForAction(T action);
    protected abstract string? GetCommandToCallAction(T action);

    protected virtual IEnumerable<uint> GetIllegalActionIDs() {
        return Array.Empty<uint>();
    }

    private static T? GetActionById(uint id) {
        // should never be null, T is inherently handled by Lumina
        return Injections.DataManager.Excel.GetSheet<T>()!.GetRow(id);
    }

    public List<ExecutableAction> GetAllowedItems() {
        if (this._actionCache.Count > 0) {
            // this is (relatively) safe as general actions shouldn't (can't) be added midway through the game.
            // so let's just cache them and return whenever this is called just to save a tiiiny amount of memory
            return this._actionCache;
        }

        ExcelSheet<T> sheet = Injections.DataManager.Excel.GetSheet<T>()!;

        if (sheet == null) {
            throw new NullReferenceException(string.Format(UIStrings.FixedCommandStrategy_SheetNotFoundError, typeof(T).Name));
        }

        foreach (var row in sheet) {
            // skip illegal action IDs
            if (row.RowId == 0) continue;
            if (this.GetIllegalActionIDs().Contains(row.RowId)) continue;

            var actionName = this.GetNameForAction(row);
            if (string.IsNullOrEmpty(actionName)) continue;

            this._actionCache.Add(new ExecutableAction() {
                ActionId = (int) row.RowId,
                ActionName = actionName,
                IconId = this.GetIconForAction(row),
                HotbarSlotType = this.GetHotbarSlotType()
            });
        }

        return this._actionCache;
    }

    public ExecutableAction? GetExecutableActionById(uint actionId) {
        // logic here is interesting, but basically comes down to that we only will ever want allowed items from
        // this type.

        return this.GetAllowedItems().Find(ac => ac.ActionId == actionId);
    }

    public void Execute(uint actionId, dynamic? options = null) {
        if (this.GetIllegalActionIDs().Contains(actionId))
            throw new ArgumentException(string.Format(UIStrings.FixedCommandStrategy_IllegalActionError, actionId));

        var action = GetActionById(actionId);

        if (action == null) {
            throw new ArgumentNullException(nameof(actionId),
                string.Format(UIStrings.FixedCommandStrategy_ActionNotFoundError, typeof(T), actionId));
        }

        var command = this.GetCommandToCallAction(action);

        if (command == null) {
            PluginLog.Warning(
                "An ExecutableAction returned without a command. This shouldn't happen, but isn't fatal either.");
            return;
        }

        PluginLog.Debug($"Would execute command: {command}");

        TickScheduler.Schedule(delegate { ChatUtils.SendSanitizedChatMessage(command); });
    }

    public int GetIconId(uint actionId) {
        var action = GetActionById(actionId);

        return action == null ? 0 : this.GetIconForAction(action);
    }
}
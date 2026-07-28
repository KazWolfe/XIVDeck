using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using XIVDeck.FFXIVPlugin.ActionExecutor.Payloads;
using ActionAppearance = XIVDeck.FFXIVPlugin.Contract.ActionAppearance;
using XIVDeck.FFXIVPlugin.Contract;

namespace XIVDeck.FFXIVPlugin.ActionExecutor;

public interface IActionStrategy {
    /// <summary>
    /// Executes the specified action ID for this strategy.
    /// </summary>
    /// <param name="actionId">The ID to execute.</param>
    /// <param name="options">Any custom parameters to include.</param>
    /// <returns>Async task.</returns>
    public Task Execute(uint actionId, ActionPayload? options = null);

    /// <summary>
    /// Get an action entry for UI purposes. Ignores lock checks.
    /// </summary>
    /// <param name="actionId">The ID to retrieve.</param>
    /// <returns>Action Entry for UI.</returns>
    public ActionEntry? GetActionEntryById(uint actionId);

    /// <summary>
    /// List all actions that should be displayed to the user via UI.
    /// </summary>
    public List<ActionEntry> GetSelectableActions();

    /// <summary>
    /// Get the hotbar slot appearance of a given action ID.
    /// </summary>
    /// <param name="actionId">The ID to look up.</param>
    /// <returns>Async task with action appearance.</returns>
    public Task<ActionAppearance> GetAppearance(uint actionId);

    /// <summary>
    /// The type used by <see cref="Execute"/>, if necessary. Used for resolving and type-casting.
    /// </summary>
    /// <returns>The type to expect options in.</returns>
    public Type? GetPayloadType() => null;
}

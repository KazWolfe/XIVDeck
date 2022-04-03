using System.Collections.Generic;

namespace XIVDeck.FFXIVPlugin.ActionExecutor;

public interface IActionStrategy {
    /**
     * Execute an event with the given Action ID, depending on the strategy for this action type.
     */
    public void Execute(uint actionId, dynamic? options = null);

    /**
     * Get the Icon ID used for a specific action type
     */
    public int GetIconId(uint actionId);

    /**
     * Get a specific action regardless of unlock state by ID
     */
    public ExecutableAction? GetExecutableActionById(uint actionId);

    /**
     * Get a dynamic list of items allowed by this strategy.
     * 
     */
    public List<ExecutableAction>? GetAllowedItems();
}
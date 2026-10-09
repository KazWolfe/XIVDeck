# Execute Game Action

This control allows the user to execute a specific in-game action by reference, such as a specific emote. Only a subset
of actions (decided by the server) may be executed.

Note that Macros will not be executed by this control, despite still being Actions.

## Parameters

- **Action Type**: A dropdown indicating the category/type of action that this control will execute.
  - The list is received from the server and is dynamic.
  - The selected value is cached in the control's config, so that the control will show a valid UI setting even if the
    game is not connected or the action is not available. The selected value in this case should be disabled.
- **Action Name**: A dropdown indicating the specific action that this control will execute.
  - The list is received from the server and is dynamic.
  - The selected value is cached in the control's config, so that the control will show a valid UI setting even if the
    game is not connected or the action is not available. The selected value in this case should be disabled.
  - Actions may additionally have a "sub-category."

Certain other actions may have additional parameters. For example:

- Emote actions allow specifying the emote execution mode.
  - **Default**: Use the game's rules for showing the emote chat message.
  - **Always**: Always send the emote's associated chat message, regardless of game settings.
  - **Never**: Never send the emote's associated chat message, regardless of game settings.
  - This setting is sent to the game client as an action parameter.
- Gearset actions allow specifying whether to use a glamour plate override.
  - Exposed as a checkbox and a text field. When checked, the user can enter a value between 1 and 20.
  - This setting is sent to the game client as an action parameter.

## Functionality

When this control is loaded onto a Stream Deck via `willAppear`, it will request its own action appearance from the
game client. It will then render the calculated appearance on the Deck. Note that for consistency's sake, this
appearance will be rendered using the same slot presentation system as present in the Hotbar and Macro controls.

Every control will also subscribe to the `Action.ActionUpdate` event, which fires when the game client determines that
the action state has changed materially. When this event fires, the control will determine whether the update event is
relevant to itself, and if so, will update its appearance accordingly. To check, the control will check whether the
Action Type matches, optionally also checking the Action ID if that value has been sent. If an Action Type is not sent,
the control will assume that the update event is relevant. This event does *not* require the control inform the game
client; it is sent unconditionally.

If the action in question has cooldown information associated with it, the control will additionally subscribe to the
relevant cooldown group IDs. See the Hotbar control for more information.

## RPC Invocations

* `Action.GetActionEntry`: Gets a single ActionEntry (used for PI display purposes) for the specified Type and ID.
  * This may be used to request information about actions that are not currently valid.
* `Action.GetActions`: Gets a list of all (valid) actions for PI display purposes.
* `Action.GetActionAppearance`: Gets the appearance for the specified Type and ID.
* `Action.ExecuteAction`: Executes the specified action.
* `Action.ActionUpdate`: Server-side notification that an action needs to be updated.
* `Icon.GetIcon`: used to fetch the current PNG of an icon by ID.
  * Should go through the per-connection caching layer.

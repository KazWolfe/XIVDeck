# Execute Macro

Allows a player to execute a specific in-game macro on button press.

This is a special-cased version of the Execute Action control.

## Parameters

* **Macro Type**: The type of macro to execute. Radio button.
  * Choice between **Individual** and **Shared** macros.
* **Macro ID**: The ID of the macro to execute. Text input, bound from 0 to 99.

## Functionality

When this control is loaded onto a Stream Deck via `willAppear`, it will request its own action appearance from the
game client. It will then render the calculated appearance on the Deck. Note that for consistency's sake, this
appearance will be rendered using the same slot presentation system as present in the Hotbar and Action controls.

This control will also (unconditionally) subscribe to the `Action.ActionUpdate` event, which will fire when the game
client deems that controls have changed significantly. This control is to update its own appearance when a notification
comes in without a specified Action Type, if the Action Type is Macro and no ID is specified, or if both the Action Type
and Action ID match the configured values.

If the action in question has cooldown information associated with it, the control will additionally subscribe to the
relevant cooldown group IDs. See the Hotbar control for more information.

## RPC Invocations

* `Action.GetActionAppearance`: Gets the appearance for the specified Type and ID.
* `Action.ExecuteAction`: Executes the specified action.
* `Action.ActionUpdate`: Server-side notification that an action needs to be updated.
* `Icon.GetIcon`: used to fetch the current PNG of an icon by ID.
  * Should go through the per-connection caching layer.

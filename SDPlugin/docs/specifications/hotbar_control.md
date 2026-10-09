# Execute Hotbar Slot

Allows a player to trigger a specific (game-native) hotbar slot by ID.

## Parameters

- **Hotbar ID:** A dropdown of all hotbars available to the player.
  - Both standard and cross hotbars are included. The dropdown shall separate these sets into two separate option
    groups.
  - Hotbars shall be listed in the format of `Hotbar N` and `Cross Hotbar N`.
  - Pet hotbars should be selectable for each standard and cross hotbars, and should be listed as `Pet Hotbar` or
    `Pet Cross Hotbar`.
- **Slot ID:** The numeric ID of the slot to execute.
  - For standard hotbars, this should be bound between 1 and 12.
  - For cross hotbars, this should be bound between 1 and 16.

## Functionality

When this control appears on a Stream Deck via `willAppear`, it will attempt to load the appearance of the specified
hotbar slot. It will also set up a listener such that it can be informed of any changes to the tracked hotbar slot. This
will additionally require informing the game that this specific slot should be tracked.

When the game detects a change to the hotbar slot's state, it will send a message to the Stream Deck indicating that the
contents of a specific tracked slot have changed. This message shall consist of only the Hotbar ID and Slot ID affected.
It is then the responsibility of the Stream Deck side to re-fetch and recompute any appearances of this control.

If the slot is determined to have cooldown information associated with it, the control will additionally subscribe to
the relevant cooldown Group IDs (which are always sent via RPC) to trigger a re-render on a 200-millisecond cadence. All
slots should re-render on the same tick.

When this control is pressed, it will send an RPC command to execute the specified hotbar slot.

If the user changes the configured slot ID, the control will indicate to the game that it is no longer tracking the
previous slot and will begin tracking the new slot. The control will then re-fetch new appearances and recompute the
display status of this slot.

On `willDisppear`, the control will indicate to the game that it is no longer tracking the slot and clean itself up.

## RPC Invocations

* `Hotbar.TriggerHotbarSlot`: used to execute the specified hotbar slot by ID.
* `Hotbar.GetHotbarSlot`: used to fetch the current appearance information and slot metadata.
* `Hotbar.SetWatchedSlots`: used to indicate which hotbar slots the game should track. Shared across an entire game
  connection.
* `Hotbar.OnSlotChanged`: server-side notification indicating that a specific slot has updated.
  * TBD: Should the full change information be re-calculated on the server side?
* `Icon.GetIcon`: used to fetch the current PNG of an icon by ID.
  * Should go through the per-connection caching layer.

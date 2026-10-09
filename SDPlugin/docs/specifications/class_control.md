# Change Class Button

This control allows users to change to a specific ClassJob via a button press.

## Parameters

* **Target Class**: A dropdown menu that allows the user to select their desired class or job.
  * Will be segmented into "categories" like "Melee", "Ranged", "Magic", etc.
  * The list of options is provided by the server.
* **Override Glamour Plate**: A checkbox that, when checked, will force a specific Glamour Plate to be set when
  switching to this class.
  * When checked, exposes a numeric input for Glamour Plate ID, bound between 1 and 20.

## Control Functionality

When this control is loaded onto a Stream Deck via `willAppear`, it will request information for the target class job
and use this to set the appropriate Icon on the button. Unlike other controls, this will *not* use the Virtual Hotbar
presenter.

When pressed, this control will send a request to the game client to change the player's current class. The request will
include the selected class job and, if applicable, the Glamour Plate ID.

This control does not need to implement any listener logic.

## RPC Invocations

* `ClassJob.GetClass`: Get information on a specific class job. Used for both PI and action workflows.
* `ClassJob.GetAvailableClasses`: Get a list of all available class jobs. Used for populating the dropdown menu.
* `ClassJob.ChangeClass`: Change the player's current class to the selected class job. Used when the button is pressed.
* `Icon.GetIcon`: used to fetch the current PNG of an icon by ID.
  * Should go through the per-connection caching layer.

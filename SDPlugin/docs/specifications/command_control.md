# Execute Command Button

This control allows users to execute an arbitrary command on the game client.

## Parameters

* **Command**: The command to execute on the game client.
  * Must always start with a `/`, and may not contain newlines.

## Control Functionality

No special behavior on initialization.

When pressed, the control will send the command to the game client.

## RPC Invocations

* `Command.ExecuteCommand`: Execute the command on the game client.

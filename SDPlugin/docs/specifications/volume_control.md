# Change Volume

This control allows users to change the volume of the game client or any specific sub-channel within the client.

Its behavior and configuration principles differ slightly between a button type and a dial type.

## Parameters

* **Channel**: A dropdown to select the channel that this control will affect.
  * Received from the server, but may have client-side logic (e.g. naming).

If this control is on a Dial:

* **Step**: The amount to change the volume by when the dial is turned.
  * Must be a positive integer, bound between 1 and 5. Defaults to 2.

If this control is on a Button:

* **Mode**: The mode of this button. Will be one of `Set`, `Adjust`, or `Mute`.
  * When `Set`, the control will expose a **Target** parameter.
  * When `Adjust`, the control will adjust the volume by a **Step** amount (numeric, from -20 to 20).
  * When `Mute`, the control will mute the channel and not require additional settings.]

## Control Functionality

When this control is registered via `willAppear`, it will need to register to the volume changed notification to be
informed of volume updates. Appropriate updates (that is, targeting the specified channel) will need to trigger a
re-render.

### Dial-Specific Behavior

When used in a Dial, this control will perform the following actions:

* Upon the dial being turned, the volume will change by the specified **Step** amount. If the dial is turned multiple
  times in a single "tick", the volume will change by the **Step** amount for each turn.
* Upon the dial being pressed, the mute state of the channel will be toggled.
* Upon the screen being tapped, the mute state of the channel will be toggled.

Upon load or volume change notification, the control shall update its display to reflect the current volume and mute
state via the Feedback interface. Feedback will use the `$B1` layout:

* The `indicator` value of the layout is used to display the volume on a meter.
  * The indicator shall always display the precise value of the volume channel.
  * When the channel is muted, the indicator shall be colored RED, otherwise WHITE.
* The `value` of the layout is used to display short textl
  * When the channel is muted, the value shall be `Muted`, otherwise it shall be a numeric representation of the
  * channel's volume.
* The `icon` value of the layout is used to display the mute state of the channel.
  * When the channel is muted, the icon display `o_muted`, otherwise `o_unmuted`.

If the game client is not connected, the control shall display the following:
* The `indicator` shall be set to `0` with an opacity of `0.6`.
* The `value` shall be set to the text string `--`.
* The `icon` shall be set to `o_nodata`.

### Button-Specific Behavior

When used in a Button, the behavior will be driven by the **Mode** parameter.

* When pressed and **Mode** is `Set`, the volume will be set to the specified **Target** value.
* When pressed and **Mode** is `Adjust`, the volume will be adjusted by the specified **Step** amount.
* When pressed and **Mode** is `Mute`, the mute state of the channel will be toggled.

Upon load, the control shall render as follows:

* By default, render state 0 ("unmuted") shall be used.
* When muted, render state 1 shall be used.
* When in `Set` mode, the title of this button shall be `{NAME} {target value}`.
* When in `Adjust` mode, the title of this button shall be `{NAME} {step value}`.
* When in `Mute` mode, the title of this button shall be the (short) channel name.

### RPC Invocations

* `Volume.GetAllChannels`: Gets all volume channels, used for PI and other states.
* `Volume.GetChannel`: Gets the current state of a volume channel.
* `Volume.SetChannel`: Sets the absolute volume of the specified channel.
* `Volume.DeltaChannel`: Adjusts the volume of the specified channel by the specified delta.
* `Volume.ToggleMuteChannel`: Toggles the mute state of the specified channel.

# Vitals Control Specification

>!NOTICE
> This control has not been fully defined yet.

This control allows the user to see vitals at a glance.

Its behavior and configuration principles differ based on the control type.

## Parameters

When used on an Info Bar:

* **Mode**: The mode of this infobar. One of the following:
  * **Health**: Shows the character's HP and MP/CP/GP/SP/TP values on two meters.
  * **Class**: Shows the character's class, level, and XP progression.
  * **Status**: Shows the character's status effects. (TBD?)
  * **Currency**: Shows a currency value. (TBD?)
  * **Pull Timer**: Shows the time since starting this pull. (TBD?)

## Functionality

This control is only available on Info Bars for now, though may be expanded to other controls in the future. This
control does not have interactive behavior and exists only as a display.

When this control is loaded onto a Stream Deck via `willAppear`, it indicates to the game client that vitals are being
displayed. The game client will then begin watching *all* vitals changes. (TBD, we may specify the type of vitals).

In **Health** mode, the control will display the character's HP and a secondary metric (determined by the game client)
on two separate meters. The server will send a `primary` and `secondary` metric value, which will then be displayed on
the two meters.

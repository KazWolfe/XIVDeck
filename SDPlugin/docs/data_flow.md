# XIVDeck SD Data Flow

This document describes the data flow within the StreamDeck side of the XIVDeck plugin.

## Service Diagram

At a high level, services are scoped as follows:

```
Plugin  (the full process lifetime, root scope)
  GlobalSettings
  ClientManager
    Client (one per server endpoint)
      GameConnection
      IconCache
      HotbarTracker
      VitalsTracker
      CooldownTracker
      ... any other per-client services that may exist ...

  ControlDispatcher (singleton)
    Control (many)
      ClientProxy
```

## RPC Connections (Clients)

The Stream Deck plugin is the RPC client; each XIVDeck RPC server lives inside a running game client (FFXIV). In code,
a `Client` is the plugin's side of the link to one server, and "game client" always means FFXIV.

Each layer is built on the one below it:

| Term          | What it is                                                                         | Lifetime                                                | Code                                       |
|---------------|------------------------------------------------------------------------------------|---------------------------------------------------------|--------------------------------------------|
| Endpoint      | An *address* where a server may be listening: pipe name, socket path, or host/port. | Configured (saved) or found by a scan                   | `Endpoint`, identified by `Endpoints.keyOf` |
| Transport     | A *channel* to an endpoint: a framed, bidirectional byte pipe.                     | One dial; discarded when closed                         | `IRpcTransport` and implementations        |
| Connection    | The *RPC session* over the current transport: handshake, requests, notifications. | The client's lifetime; re-opened over a new transport   | `GameConnection`                           |
| Client        | The plugin's side of *one server*: the connection plus per-game state and services. | While its endpoint is a candidate, across reconnects    | `Client`                                   |
| ClientManager | Decides which clients exist (config + scan) and when they dial.                    | The plugin's lifetime                                   | `ClientManager`                            |

A control reaches its client through a `ClientProxy` (`this.client`).

The plugin connects to every XIVDeck RPC server it knows of: each saved connection, plus every pipe/socket found while
discovery is enabled. There is one `Client` per endpoint, identified by its connection string (e.g.
`uds:/tmp/XIVDeck-1234.sock`). Controls use the most recently focused client. See [transports.md](transports.md).

A client lives as long as its endpoint stays a candidate: until its saved connection is deleted, or until its
pipe/socket disappears (or discovery is turned off). Each client is independently responsible for its own connection
state and error handling.

Each client will be able to provide services scoped to its own lifetime. As an example:

* `CooldownTicker` serves to receive cooldown group information from the game plugin.
  * Cooldown events are always sent to this service, regardless of subscription.
* `HotbarTracker` serves to manage which hotbar slots need to be listened to.
* `IconCache` serves to manage the icon cache for the client.

## Action (Control) Lifecycle

Stream Deck Actions (otherwise known as Controls in this plugin, since FFXIV already uses the term "action"), have an
independent lifecycle to RPC connections or clients. They are instantiated on `willAppear`, and destroyed on
`willDisappear`. A Control will only connect to a single client, but may switch between clients during its lifetime.

**When switching clients, it's imperative that the Control start using the services in the new client's scope.**

Certain Controls may also need to subscribe to events from the game, such as hotbar updates. In some cases, these
events may need to be "enabled" by use of some command:

* To listen to a hotbar slot update, the Control must actively request its slot be added to the watch list.
  * This is done by informing the bound client that a certain slot is to be watched, at which point the client will
    send a fresh `Hotbar.SetWatchedSlots` command to the RPC server.
  * If the Control changes its bound slot or client, any old subscription(s) must be removed.

## Configuration Lifecycle

Both global settings and Controls may have individual configurations applied to them. In all cases, configurations are
sent/received via the Elgato API (`getSettings`/`setSettings`/`getGlobalSettings`/`setGlobalSettings`, etc.).

Global settings contain the following:

* A list of all clients configured
* Feature toggles
* Other plugin-wide settings

Controls will have settings that are often per-control-type, so the shape of which may vary depending on what is needed.

In all cases, however, configurations need to support the ability to be migrated between different versions. However,
as far as any systems are concerned, migrations are to be transparent to business logic. This means, as an example,
that any and all migrations would need to be performed by a `ControlFactory` rather than by the control itself.

## Writing a Control

Controls are pure business logic. A control talks to the game only through its `ClientProxy`
(`this.client`), which hides which `Client` it is bound to and what happens when that changes. Each
control gets its own DI scope (built by `ControlFactory`) containing the control, its `ClientProxy` and the binding
services.

* **Declare subscriptions once**, in the constructor, on injected binding services: `GameNotificationProxy.on(...)`,
  `HotbarTrackerProxy.changed`, `CooldownTrackerProxy.refreshed`. Controls inject only the ones they use (they are constructed
  lazily in the control's scope). Each registers itself with the `ClientProxy`, which carries it across client
  switches and detaches it on dispose, so controls do not write teardown code. Adding a new per-client concern means
  one new binding service plus one binding line in `ControlFactory`.
* **Move parameterized watches in `onSettingsChanged`**, e.g. `hotbars.watch(slot)` / `release()`.
* **Draw in `render()`**. The base class re-runs it on settings changes
  and whenever the game becomes available or unavailable, merges overlapping render requests, and logs failures. Call
  `this.requestRender()` from handlers that need a redraw.
* **Slot-style controls inject a `VirtualSlotPresenter`** (Hotbar, Exec Action, Macro): `render()` fetches an appearance and
  hands it to `slot.show(appearance, typeIconSvg?)`, or calls `slot.clear()` when unconfigured. From there the slot
  keeps the image current: it resolves the icon, watches the shown action's cooldowns, and redraws on every
  `cooldowns.tick` and on `cooldowns.refreshed` (the proxy re-fetches only the cooldown details, not the whole
  appearance, when a watched group restarts). The slot knows nothing about the device: the control passes a handler
  to `slot.setPresentHandler(...)` in its constructor and puts each new image wherever it belongs (`setImage` on a key,
  a feedback item on a dial or infobar).
* **Handle input** by implementing `IKeyControl` (`onKeyDown`, plus the optional `onKeyUp`) and/or `IDialControl`
  (`onDialDown`, `onDialRotate`, `onTouchTap`). Each handler gets the Stream Deck event for its input details (e.g.
  `ev.payload.ticks`); read settings from `this.settings`, not the event. Throwing shows an alert on the device and
  logs. The dispatcher ignores input a control does not support.
* **Display-only controls** (e.g. on the Stream Deck Neo infobar, which sends no input) implement neither interface.
  Take the typed action with `context.asInfobar()` and draw through `setFeedback` / `setFeedbackLayout`.
* **Register the control** in `ControlFactory`'s table, with its settings migration chain and one class per controller
  type; the table's types require `Keypad` classes to implement `IKeyControl` and `Encoder` classes `IDialControl`,
  while `Neo` classes are display-only.
  Controls never see old settings versions; `this.settings` is `undefined` until the control is configured.

Under the hood, each binding service (`GameNotificationProxy`, `HotbarTrackerProxy`, `CooldownTrackerProxy`) is an `IProxyService`
that registers itself with the control's proxy and remembers what the control wants. When the proxy changes clients,
it `detach`es every binding from the old client scope and `attach`es it to the new one. Supporting a new per-client
service means adding one binding.

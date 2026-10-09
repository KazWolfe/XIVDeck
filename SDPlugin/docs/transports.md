# Transport Configuration

XIVDeck supports the concept of having multiple "transports" (clients) configured at once. This is not something most
users will have, but it's worth documenting out.

## Theory of Operation

A key component of the XIVDeck 0.5.0 release is the idea that the communication layer with the game is far more
flexible. XIVDeck's server may communicate over a Named Pipe (on Windows), UNIX Domain Socket (UDS, on *nix), or a
WebSocket (WS, on any OS) connection. Normally, XIVDeck should be able to automatically configure itself through
discovery and other tools.

However, there are cases where manual configuration may be necessary, such as:

* Unix hosts that can't easily place files in `/tmp` or have other access constraints.
* Systems that require a WebSocket connection on a non-standard port.

There are also various use cases for multiple simultaneous servers. While most users will only run a single game client,
there needs to be some ability to have a more flexible configuration. Examples of such cases include:

* A setting to "follow focus", where the Stream Deck plugin will automatically target the last-focused game client, as
  reported by the game client(s) themselves.
* The ability to configure multiple connections manually, assigning a specific connection to each control.
  * Controls that do not target a specific connection will inherit "global" settings. Note that there is nothing special
    about the global settings as far as the plugin is concerned, it's just the default for controls that don't target a
    specific connection.
  * At present, this is not directly possible as the game client will name their sockets using pattern `XIVDeck-{pid}`,
    which is inherently unpredictable. In the future, it will be possible to configure a game client to use a specific
    socket name.

## Intended Design Goals

The most important design goal is that XIVDeck should work for as many users as possible without any configuration at
all. In order, the behavior should be as follows (ordered by design phase):

1. XIVDeck will automatically connect to the first game client it sees, and use that for all controls.
2. XIVDeck offers the ability to manually configure the default connection (autodiscovery rules?).
   1. This will be exposed via an in-UI option to select either Named Pipe/UDS or a WebSocket connection, and
      configuring the appropriate settings (e.g. directory to the socket path for UDS, or WebSocket port).
   2. At this phase, autodiscovery rules still apply, but only scoped to the narrow(er) set of rules chosen by the user.
3. Autodiscovery will connect to *all* clients it sees (and continually search for new ones) and use the last focused
   client **by default**.
4. XIVDeck offers the ability to add additional connections that are stored in a list.
   1. Controls may target a specific connection, or if none are selected, the default connection.
   2. Autodiscovery and manual connections are not mutually exclusive, unless the default client is set to use a
      specific configuration.

In effect, there are two orthogonal axes for configuration: the autodiscovery configuration and the manual control
configuration. However, the two cross with WebSockets, as those can't be "discovered" automatically outside of the
default port configuration. As such, creating a WebSocket connection with a custom port is *effectively* the same as
creating a manual connection.

This also creates a bit of complexity with autodiscovery and connection rules in general: if a pipe is connected, a
WebSocket may still appear later. Likewise, if a WebSocket is connected, it's not a guarantee that a future pipe may
not appear.

## Configuration

Transport configuration lives in the plugin's global settings. A game client will only ever expose a single server
type, so a game is reachable through exactly one endpoint, and endpoints (not game clients) are the unit of
configuration.

```ts
type Endpoint =
    | { kind: "pipe"; name: string }               // Windows named pipe
    | { kind: "uds"; path: string }                // *nix domain socket
    | { kind: "ws"; host: string; port?: number }; // port omitted = current default port

interface SavedConnection {
    id: string;
    name: string;
    endpoint: Endpoint;
}

interface TransportSettings {
    discovery: {
        enabled: boolean;
        udsScanPaths: string[];
    };
    savedConnections: SavedConnection[];
}
```

### Discovery

* While `discovery.enabled` is set, the plugin scans for `XIVDeck-*` pipes (Windows) or sockets (*nix) every few
  seconds. This is the future "autodiscover clients" option for multi-client setups.
* `udsScanPaths` is the list of directories scanned for sockets on *nix. An empty list means the built-in default
  locations; a non-empty list *replaces* the defaults rather than adding to them. It is ignored on Windows, where named
  pipes are discovered from the pipe namespace.
* WebSocket servers are never discovered. Like the game, which only uses WebSocket when the user selects it, a
  WebSocket endpoint must be a saved connection.

### Saved Connections

* Saved connections are the user's list of explicit endpoints of any kind. Every saved connection is always connected.
  There is no "disabled" state; a connection that should not be used is deleted.
* WebSocket mode is a saved connection with the reserved ID `default_ws`, created when that mode is chosen:
  ```ts
  { id: "default_ws", name: "Default WebSocket", endpoint: { kind: "ws", host: "localhost" } }
  ```
  Its port is left unset when it is the default, so that a change to the default port does not require a settings
  migration.

### Clients

The plugin keeps one client per endpoint, identified by its connection string (e.g. `uds:/tmp/XIVDeck-1234.sock`,
`ws:localhost:37984`). Candidates are built in two steps:

1. **Find:** every saved connection, then (while discovery is enabled) every scanned pipe/socket.
2. **Deduplicate** by endpoint, keeping the first. Saved connections come first, so they claim their endpoints and a
   scan result for a saved endpoint is dropped.

The result is in discovery order: saved connections in list order, then scanned sockets oldest first. Candidates, and so
clients, exist only while a game process is running; each client keeps itself connected for as long as it exists.

### Default Client

Controls use the ready client with the latest `lastFocusTime`. Never-focused clients rank last, and ties go to discovery
order; with a single game connected, that game is simply the default. If the target disconnects, the next most
recently focused ready client takes over. If no client is ready, controls are unavailable.

Game clients report focus with the `GameState.FocusChanged` notification, and the plugin reads the initial state with
`GameState.GetFocusState` on connect.

A control may later be pointed at a specific saved connection instead (`ClientProxy.setTargetClient`); this is not
exposed yet.

## v0.5.0 Scope

v0.5.0 ships global configuration only. Per-control connection targets and user-defined saved connections (other than
`default_ws`) are not exposed. Every control uses the default client.

The global settings present a single **Connection** mode selector. The mode is not stored; it is derived from the
settings, and each mode change writes a complete `TransportSettings` in a single save.

| Mode                      | `discovery.enabled` | `udsScanPaths`              | `savedConnections`  |
|---------------------------|---------------------|-----------------------------|---------------------|
| **Automatic** (default)   | `true`              | User list (`[]` = defaults) | `[]`                |
| **WebSocket**             | `false`             | Preserved, ignored          | `[default_ws]`      |

Mode is derived as follows: discovery enabled with no saved connections is **Automatic**; discovery disabled with only
a WebSocket `default_ws` is **WebSocket**. Settings that match neither (e.g., hand-edited) are surfaced as such rather
than coerced.

* Selecting **WebSocket** creates `default_ws`; selecting **Automatic** removes it, so a custom port does not survive a
  trip through **Automatic**.
* Scan paths are only editable in **Automatic** mode, and survive **WebSocket** mode. On Windows there are none.
* The WebSocket host is fixed to `localhost` in v0.5.0; only the port is editable.
* Settings migrated from v0 with a customized WebSocket port start in **WebSocket** mode; everything else starts in
  **Automatic**. This matches the game, which keeps WebSocket only for users who customized the port.

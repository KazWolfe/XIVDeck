import {JsonObject} from "@elgato/utils";

/** The port a WebSocket endpoint without one uses. Matches the game's default. */
export const DEFAULT_WS_PORT: number = 37984;

export type Endpoint =
    | { kind: "pipe"; name: string }                // Windows named pipe
    | { kind: "uds"; path: string }                 // *nix domain socket
    | { kind: "ws"; host: string; port?: number };  // port omitted = current default port

export interface SavedConnection extends JsonObject {
    id: string;
    name: string;
    endpoint: Endpoint;
}

export interface TransportSettings extends JsonObject {
    discovery: {
        enabled: boolean;
        udsScanPaths: string[];
    };
    savedConnections: SavedConnection[];
}

export class Endpoints {
    /**
     * A stable, human-readable identity for an endpoint (e.g. `uds:/tmp/XIVDeck-1234.sock`). Endpoints with the same
     * key are the same server; the client layer uses it to deduplicate candidates, as client IDs, and in logs.
     */
    public static keyOf(endpoint: Endpoint): string {
        switch (endpoint.kind) {
            case "ws":
                return `ws:${endpoint.host}:${endpoint.port ?? DEFAULT_WS_PORT}`;
            case "pipe":
                return `pipe:${endpoint.name}`;
            case "uds":
                return `uds:${endpoint.path}`;
        }
    }
}

export class TransportDefaults {
    public static readonly DEFAULT_WS_ID = "default_ws";

    /** Automatic: discover pipes/sockets, nothing saved. */
    public static create(): TransportSettings {
        return {
            discovery: {enabled: true, udsScanPaths: []},
            savedConnections: [],
        };
    }

    /** The connection WebSocket mode creates. */
    public static createDefaultWebSocket(): SavedConnection {
        return {
            id: TransportDefaults.DEFAULT_WS_ID,
            name: "Default WebSocket",
            endpoint: {kind: "ws", host: "localhost"},
        };
    }
}

import {DEFAULT_WS_PORT, TransportDefaults} from "#/settings/types/TransportSettings";
import type {Endpoint, SavedConnection, TransportSettings} from "#/settings/types/TransportSettings";

export type ConnectionMode = "automatic" | "websocket";

type WebSocketEndpoint = Extract<Endpoint, { kind: "ws" }>;

export class ConnectionModes {
    public static derive(transport: TransportSettings): ConnectionMode | undefined {
        const defaultWs = ConnectionModes.findDefaultWs(transport);
        const others = ConnectionModes.withoutDefaultWs(transport);
        if (others.length > 0) {
            return undefined;
        }

        if (transport.discovery.enabled && !defaultWs) {
            return "automatic";
        }

        if (!transport.discovery.enabled && defaultWs && ConnectionModes.webSocketEndpoint(defaultWs)) {
            return "websocket";
        }

        return undefined;
    }

    public static apply(mode: ConnectionMode, transport: TransportSettings): TransportSettings {
        switch (mode) {
            case "automatic":
                return ConnectionModes.withScanPaths(transport, [...transport.discovery.udsScanPaths]);
            case "websocket":
                return ConnectionModes.webSocketSettings(transport, ConnectionModes.currentEndpoint(transport));
        }
    }

    public static portOf(transport: TransportSettings): number {
        return ConnectionModes.currentEndpoint(transport).port ?? DEFAULT_WS_PORT;
    }

    public static withPort(transport: TransportSettings, port: number): TransportSettings {
        return ConnectionModes.webSocketSettings(transport, {
            ...ConnectionModes.currentEndpoint(transport),
            port: port === DEFAULT_WS_PORT ? undefined : port,
        });
    }

    public static withScanPaths(transport: TransportSettings, udsScanPaths: string[]): TransportSettings {
        return {
            discovery: {enabled: true, udsScanPaths},
            savedConnections: ConnectionModes.withoutDefaultWs(transport),
        };
    }

    private static webSocketSettings(transport: TransportSettings, endpoint: WebSocketEndpoint): TransportSettings {
        return {
            discovery: {enabled: false, udsScanPaths: [...transport.discovery.udsScanPaths]},
            savedConnections: [
                {...TransportDefaults.createDefaultWebSocket(), endpoint},
                ...ConnectionModes.withoutDefaultWs(transport),
            ],
        };
    }

    private static currentEndpoint(transport: TransportSettings): WebSocketEndpoint {
        const defaultWs = ConnectionModes.findDefaultWs(transport);
        const endpoint = defaultWs ? ConnectionModes.webSocketEndpoint(defaultWs) : undefined;

        return {...(endpoint ?? {kind: "ws", host: "localhost"})};
    }

    private static findDefaultWs(transport: TransportSettings): SavedConnection | undefined {
        return transport.savedConnections.find(connection => connection.id === TransportDefaults.DEFAULT_WS_ID);
    }

    private static withoutDefaultWs(transport: TransportSettings): SavedConnection[] {
        return transport.savedConnections.filter(connection => connection.id !== TransportDefaults.DEFAULT_WS_ID);
    }

    private static webSocketEndpoint(connection: SavedConnection): WebSocketEndpoint | undefined {
        return connection.endpoint.kind === "ws" ? connection.endpoint : undefined;
    }
}

import {MigrationChain} from "#/settings/SettingsMigrator";
import {GlobalSettings} from "#/settings/GlobalSettings";
import {DEFAULT_WS_PORT, TransportDefaults} from "#/settings/types/TransportSettings";

interface GlobalSettingsV0 {
    ws?: {
        hostname?: string;
        port?: number;
    };
}

const DEFAULT_WS_HOSTNAME = "localhost";

export const GlobalSettingsMigrations: MigrationChain<GlobalSettings> = {
    currentVersion: 1,
    steps: {
        0: (v0: GlobalSettingsV0): GlobalSettings => {
            const hostname = v0.ws?.hostname || DEFAULT_WS_HOSTNAME;
            const port = v0.ws?.port ?? DEFAULT_WS_PORT;
            const transport = TransportDefaults.create();

            // v0 always stored a WebSocket host/port. The game only keeps WebSocket for users who changed the port, so
            // only they move to WebSocket mode; everyone else starts in Automatic.
            if (hostname !== DEFAULT_WS_HOSTNAME || port !== DEFAULT_WS_PORT) {
                const defaultWs = TransportDefaults.createDefaultWebSocket();
                defaultWs.endpoint = {kind: "ws", host: hostname, port: port === DEFAULT_WS_PORT ? undefined : port};

                transport.discovery.enabled = false;
                transport.savedConnections = [defaultWs];
            }

            return {_v: 1, transport};
        },
    },
};

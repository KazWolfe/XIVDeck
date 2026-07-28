import {SettingsGateway} from "../SettingsGateway";
import {DEFAULT_TRANSPORT_ID, GlobalSettings} from "../GlobalSettings";
import {WebSocketTransportConfig} from "../types/TransportConfig";
import {DEFAULT_WS_PORT} from "../../rpc/transports/WebSocketTransport";

interface GlobalSettingsV0 {
    ws: {
        hostname?: string;
        port: number;
    };
}

SettingsGateway.register<GlobalSettings>("global", {
    currentVersion: 1,
    steps: {
        0: (v0: GlobalSettingsV0): GlobalSettings => ({
            _v: 1,
            transports: [{
                id: DEFAULT_TRANSPORT_ID,
                label: "WebSocket",
                type: "websocket",
                hostname: v0.ws?.hostname || "localhost",
                port: v0.ws?.port ?? DEFAULT_WS_PORT,
            } satisfies WebSocketTransportConfig],
        }),
    },
});

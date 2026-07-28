import {JsonObject} from "@elgato/utils";

export interface TransportConfig extends JsonObject {
    id: string;
    label: string;
}

export interface WebSocketTransportConfig extends TransportConfig {
    type: "websocket";
    hostname?: string;
    port?: number;
}

export interface UDSTransportConfig extends TransportConfig {
    type: "uds";
    filename?: string;
}

export interface NamedPipeTransportConfig extends TransportConfig {
    type: "namedPipe";
    name?: string;
}

export type AnyTransportConfig = WebSocketTransportConfig | NamedPipeTransportConfig | UDSTransportConfig;

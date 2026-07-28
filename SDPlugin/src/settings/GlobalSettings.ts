import {JsonObject} from "@elgato/utils";
import {AnyTransportConfig} from "./types/TransportConfig";


export interface GlobalSettings extends JsonObject {
    _v: 1;

    /** A list of configured transports.
     *  Multiple transports can be defined to allow future expansion to support multiple connections at once.
     **/
    transports: AnyTransportConfig[];

    /** ID of the transport to use, or `null` to auto-detect the active transport. */
    chosenTransport?: string | null;
    trackCooldowns?: boolean;
}

export const DEFAULT_TRANSPORT_ID = "default";

export const DefaultGlobalSettings: GlobalSettings = {
    _v: 1,
    transports: [],
    chosenTransport: null,
    trackCooldowns: true
};

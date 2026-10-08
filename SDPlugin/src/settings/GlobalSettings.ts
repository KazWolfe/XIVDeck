import {JsonObject} from "@elgato/utils";
import {TransportSettings, TransportDefaults} from "./types/TransportSettings";


export interface GlobalSettings extends JsonObject {
    _v: 1;
    transport: TransportSettings;
    trackCooldowns?: boolean;
}

export const DefaultGlobalSettings: GlobalSettings = {
    _v: 1,
    transport: TransportDefaults.create(),
    trackCooldowns: true
};

import {JsonObject} from "@elgato/utils";

export enum EmoteLogMode {
    DEFAULT = "default",
    ALWAYS = "always",
    NEVER = "never",
}

export interface EmotePayload extends JsonObject {
    logMode?: EmoteLogMode;
}

export interface GearsetPayload extends JsonObject {
    glamourPlateId?: number;
}

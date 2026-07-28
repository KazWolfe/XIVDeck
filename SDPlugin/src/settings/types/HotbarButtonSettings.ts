import {JsonObject} from "@elgato/utils";

export interface HotbarButtonSettings extends JsonObject {
    _v: 1;
    hotbarId: number;
    slotId: number;
}

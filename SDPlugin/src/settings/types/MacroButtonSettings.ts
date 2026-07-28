import {JsonObject} from "@elgato/utils";

export interface MacroButtonSettings extends JsonObject {
    /** Individual macros are 0-99, shared 100-199. Mirrors `MacroId` on the game side. */
    macroId: number;
}

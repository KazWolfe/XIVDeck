import {JsonObject} from "@elgato/utils";
import {SerializableGameClass} from "../../rpc/messages/ClassJob";

export interface ClassButtonSettings extends JsonObject {
    classId: number;

    cache?: SerializableGameClass;
}

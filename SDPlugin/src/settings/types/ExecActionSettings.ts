import {JsonObject, JsonValue} from "@elgato/utils";
import {ActionEntry} from "#/client/rpc/messages/Action";

export interface ExecActionSettings extends JsonObject {
    _v: 1;
    actionType: string;
    actionId: number;
    payload?: JsonValue;

    cache?: ActionEntry;
}

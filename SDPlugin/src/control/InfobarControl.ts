import {Action, NeoInfobarAction, WillAppearEvent} from "@elgato/streamdeck";
import {JsonObject} from "@elgato/utils";
import {BaseControl} from "./BaseControl";
import {ClientProvider} from "../rpc/ClientProvider";

function asInfobar<TSettings extends JsonObject>(action: Action<TSettings>): NeoInfobarAction<TSettings> {
    if (!action.isNeoInfobar()) {
        throw new Error(`Action ${action.manifestId} (${action.id}) is not on a Neo infobar.`);
    }

    return action;
}

export abstract class InfobarControl<TSettings extends JsonObject> extends BaseControl<TSettings, NeoInfobarAction<TSettings>> {
    protected constructor(ev: WillAppearEvent<TSettings>, clients: ClientProvider, settingsKind: string) {
        super(asInfobar(ev.action), clients, settingsKind);
    }
}

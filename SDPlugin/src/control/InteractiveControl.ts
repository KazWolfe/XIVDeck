import {
    Action,
    DialAction,
    DialDownEvent,
    DialRotateEvent,
    KeyAction,
    KeyDownEvent,
    TouchTapEvent,
    WillAppearEvent
} from "@elgato/streamdeck";
import {JsonObject} from "@elgato/utils";
import {BaseControl} from "./BaseControl";
import {ClientProvider} from "../rpc/ClientProvider";

export type InteractiveAction<TSettings extends JsonObject> = KeyAction<TSettings> | DialAction<TSettings>;

function asInteractive<TSettings extends JsonObject>(action: Action<TSettings>): InteractiveAction<TSettings> {
    if (!(action.isKey() || action.isDial())) {
        throw new Error(`Action ${action.manifestId} (${action.id}) is not on a key or dial.`);
    }

    return action;
}

export abstract class InteractiveControl<TSettings extends JsonObject> extends BaseControl<TSettings, InteractiveAction<TSettings>> {
    private _lastImage?: string;

    protected constructor(ev: WillAppearEvent<TSettings>, clients: ClientProvider, settingsKind: string) {
        super(asInteractive(ev.action), clients, settingsKind);
    }

    protected async setImage(image: string): Promise<void> {
        if (image === this._lastImage) return;

        await this.action.setImage(image);
        this._lastImage = image;
    }

    abstract onKeyDown(ev: KeyDownEvent<TSettings>): Promise<void>;

    async onDialDown(_ev: DialDownEvent<TSettings>): Promise<void> {
    }

    async onTouchTap(_ev: TouchTapEvent<TSettings>): Promise<void> {
    }

    async onDialRotate(_ev: DialRotateEvent<TSettings>): Promise<void> {
    }

    async handleTriggerError(err: unknown): Promise<void> {
        await this.action.showAlert();
        this.logError("handling trigger for", err);
    }
}

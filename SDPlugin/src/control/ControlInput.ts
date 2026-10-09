import type {
    DialDownEvent,
    DialUpEvent,
    DialRotateEvent,
    KeyDownEvent,
    KeyUpEvent,
    TouchTapEvent
} from "@elgato/streamdeck";
import type {JsonObject} from "@elgato/utils";

// Handlers get the raw Stream Deck event for its input details (coordinates, ticks, tap position, ...).
// Its payload settings are unmigrated: read settings from the control, not the event.

export interface IKeyControl {
    onKeyDown?(ev: KeyDownEvent<JsonObject>): Promise<void>;

    onKeyUp?(ev: KeyUpEvent<JsonObject>): Promise<void>;
}

export interface IDialControl {
    onDialDown?(ev: DialDownEvent<JsonObject>): Promise<void>;

    onDialUp?(ev: DialUpEvent<JsonObject>): Promise<void>;

    onDialRotate?(ev: DialRotateEvent<JsonObject>): Promise<void>;

    onTouchTap?(ev: TouchTapEvent<JsonObject>): Promise<void>;
}

export class ControlInput {
    public static isKeyControl(control: object): control is IKeyControl {
        const candidate = control as Partial<IKeyControl>;

        return typeof candidate.onKeyDown === "function"
            || typeof candidate.onKeyUp === "function";
    }

    public static isDialControl(control: object): control is IDialControl {
        const candidate = control as Partial<IDialControl>;

        return typeof candidate.onDialDown === "function"
            || typeof candidate.onDialUp === "function"
            || typeof candidate.onDialRotate === "function"
            || typeof candidate.onTouchTap === "function";
    }
}

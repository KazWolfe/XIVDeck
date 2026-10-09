import {JsonObject, JsonValue} from "@elgato/utils";

export interface ActionInfo<TSettings extends JsonObject = JsonObject> {
    action: string;
    context: string;
    device?: string;
    payload: {
        settings: TSettings;
        coordinates?: { column: number; row: number };
        controller?: "Keypad" | "Encoder";
    };
}

export interface RegistrationInfo {
    application: { version: string; language: string; platform: string };
}

type Listener<T> = (data: T) => void;

/**
 * Minimal implementation of the Stream Deck API as the Property Inspector doesn't get one as part of the standard SDK.
 */
export class StreamDeckSocket<TSettings extends JsonObject = JsonObject> {
    private readonly _ws: WebSocket;
    private readonly _uuid: string;
    private readonly _actionInfo: ActionInfo<TSettings>;
    private readonly _info: RegistrationInfo;

    private _onDidReceiveSettings?: Listener<TSettings>;
    private _onDidReceiveGlobalSettings?: Listener<JsonObject>;
    private _onSendToPropertyInspector?: Listener<JsonValue>;

    private constructor(port: number, uuid: string, registerEvent: string, info: RegistrationInfo, actionInfo: ActionInfo<TSettings>, onOpen: () => void) {
        this._uuid = uuid;
        this._info = info;
        this._actionInfo = actionInfo;

        this._ws = new WebSocket(`ws://127.0.0.1:${port}`);
        this._ws.onopen = () => {
            this._ws.send(JSON.stringify({event: registerEvent, uuid}));
            onOpen();
        };
        this._ws.onmessage = ev => this._onMessage(JSON.parse(ev.data));
    }

    static waitForConnection<TSettings extends JsonObject = JsonObject>(): Promise<StreamDeckSocket<TSettings>> {
        return new Promise(resolve => {
            (window as any).connectElgatoStreamDeckSocket = (
                port: string, uuid: string, registerEvent: string, info: string, actionInfo: string,
            ) => {
                const socket: StreamDeckSocket<TSettings> = new StreamDeckSocket<TSettings>(
                    Number(port), uuid, registerEvent, JSON.parse(info), JSON.parse(actionInfo),
                    () => resolve(socket),
                );
            };
        });
    }

    get actionInfo(): ActionInfo<TSettings> {
        return this._actionInfo;
    }

    get info(): RegistrationInfo {
        return this._info;
    }

    getSettings(): void {
        this._send("getSettings", {});
    }

    setSettings(settings: TSettings): void {
        this._send("setSettings", {}, settings);
    }

    getGlobalSettings(): void {
        this._send("getGlobalSettings", {});
    }

    setGlobalSettings(settings: JsonObject): void {
        this._send("setGlobalSettings", {}, settings);
    }

    sendToPlugin(payload: JsonValue): void {
        this._send("sendToPlugin", {action: this._actionInfo.action}, payload);
    }

    openUrl(url: string): void {
        this._send("openUrl", {}, {url});
    }

    onDidReceiveSettings(listener: Listener<TSettings>): void {
        this._onDidReceiveSettings = listener;
    }

    onDidReceiveGlobalSettings(listener: Listener<JsonObject>): void {
        this._onDidReceiveGlobalSettings = listener;
    }

    onSendToPropertyInspector(listener: Listener<JsonValue>): void {
        this._onSendToPropertyInspector = listener;
    }

    private _send(event: string, extra: Record<string, unknown>, payload?: JsonValue): void {
        this._ws.send(JSON.stringify({event, context: this._uuid, ...extra, ...(payload !== undefined ? {payload} : {})}));
    }

    private _onMessage(message: { event: string; payload?: JsonValue }): void {
        switch (message.event) {
            case "didReceiveSettings":
                this._onDidReceiveSettings?.((message.payload as { settings: TSettings }).settings);
                break;
            case "didReceiveGlobalSettings":
                this._onDidReceiveGlobalSettings?.((message.payload as { settings: JsonObject }).settings);
                break;
            case "sendToPropertyInspector":
                this._onSendToPropertyInspector?.(message.payload as JsonValue);
                break;
        }
    }
}

import {EventEmitter} from "../../util/EventEmitter";
import {FFXIVOpcode} from "./MessageBase";
import {InitOpcode} from "./messages/outbound/InitOpcode";
import AbstractStreamdeckConnector from "@rweich/streamdeck-ts/dist/AbstractStreamdeckConnector";
import {FFXIVInitReply} from "./GameTypes";

export class FFXIVPluginLink {
    public static instance: FFXIVPluginLink;

    // static so that event registrations can survive re-initialization
    static eventManager: EventEmitter = new EventEmitter();

    // settings
    public port: number = 37984;
    public isGameAlive: boolean = false;
    public apiKey: string = "";
    
    // internal state
    private _websocket: WebSocket | null = null;
    private _plugin: AbstractStreamdeckConnector;
    private _doConnectionRetries: boolean = true;

    constructor(instance: AbstractStreamdeckConnector) {
        this._plugin = instance;
        FFXIVPluginLink.instance = this;
    }

    get baseUrl(): string {
        return `http://localhost:${this.port}`;
    }

    public async send(payload: FFXIVOpcode): Promise<unknown> {
        if (!this.isGameAlive || !this._websocket) {
            return Promise.reject(Error("The game does not appear to be alive, or a websocket has not been created yet."));
        }

        return this._websocket.send(JSON.stringify(payload));
    }

    public connect(doRetry: boolean = true): void {
        // if the game currently isn't alive, there's nothing for us to do.
        if (!this.isGameAlive) {
            console.warn("[XIVDeck - FFXIVLink] Attempted websocket connection while game should be dead.");
            return;
        }

        if (this._websocket != null) {
            console.warn("[XIVDeck - FFXIVLink] A connect was called with an already-existing websocket!");
            return;
        }

        this._doConnectionRetries = doRetry;
        this._websocket = new WebSocket(`ws://localhost:${this.port}/ws`);

        this._websocket.onopen = () => {
            let pInfo = this._plugin.info.plugin as Record<string, string>;

            this.send(new InitOpcode(pInfo.version));
            this.emit("_wsOpened", null);

            this.isGameAlive = true;
        };

        this._websocket.onmessage = this._onWSMessage.bind(this);
        this._websocket.onclose = this._onWSClose.bind(this);

        
        this.on("initReply", this._onInitReceive.bind(this));
    }
    
    private _onInitReceive(data: FFXIVInitReply) {
        this.apiKey = data.apiKey;
        
        this.emit("_ready", null)
    }

    private _onWSMessage(event: MessageEvent): void {
        let data = JSON.parse(event.data);

        console.debug("[XIVDeck - FFXIVPluginLink] Got message from websocket", data);

        if (!data.hasOwnProperty('messageType')) {
            console.warn("[XIVDeck - FFXIVPluginLink] Malformed message from XIV Plugin, missing messageType", data);
            return;
        }

        this.emit(data.messageType, data);
    }

    private _onWSClose(event: CloseEvent): void {
        console.debug("[XIVDeck - FFXIVPluginLink] Connection to WebSocket server lost!");
        this.emit("_wsClosed", {});
        this._websocket = null;
        
        if (!this._doConnectionRetries || !this.isGameAlive) return;

        if (event.code == 1002 || event.code == 1008) {
            console.warn("The WebSocket server requested we not retry connection.");
            return;
        }

        // retry a connection after 500 milliseconds, specifying connection retry logic.
        // because of how websocket closes work, a failed connection will emit a close and will hit this repeatedly.
        setTimeout(this.connect.bind(this), 500);
    }

    /**
     * Immediately close the websocket, disabling further attempts at connection retry.
     */
    public close(): void {
        this._doConnectionRetries = false;

        if (this._websocket != null) {
            this._websocket.close();
            this._websocket = null;
        }
    }

    public gracefulClose(): void {
        this._websocket?.close();
    }

    public isReady(): boolean {
        return ((this._websocket != null) && (this._websocket.readyState == 1));
    }

    public on(name: string, fn: Function): Function | undefined {
        return FFXIVPluginLink.eventManager.on(name, fn);
    }

    private emit(name: string, data: any): void {
        FFXIVPluginLink.eventManager.emit(name, data);
    }
}
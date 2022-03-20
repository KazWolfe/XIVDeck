﻿import {EventEmitter} from "../../util/EventEmitter";
import {FFXIVOpcode} from "./MessageBase";
import {InitOpcode} from "./messages/outbound/InitOpcode"
import { v4 as uuidv4 } from 'uuid';
import {WebSocketAsPromisedFaster} from "../../../lib/WebSocketAsPromisedFaster"

import WebSocketAsPromised = require("websocket-as-promised");

export class FFXIVPluginLink {
    // static so that event registrations can survive re-initialization
    static eventManager: EventEmitter = new EventEmitter();
    
    public port: number = 37984;
    private _websocket: WebSocketAsPromised | null = null;
    
    isGameAlive: boolean = false;
    private _doConnectionRetries: boolean = true;
    
    constructor() { }
    
    private static _attachContext(data: object, requestId: string | number): object {
        return Object.assign({"context": {"requestId": requestId}}, data);
    }
    
    private static _retrieveContext(data: any) : string | number | undefined {
        if (typeof(data) != "object") return undefined;
        if (data['context'] == undefined || typeof(data['context']) != "object") return undefined;
        if (data['context']['requestId'] == undefined) return undefined;

        return data['context']['requestId'];
    }
    
    
    public async send(payload: FFXIVOpcode): Promise<unknown> {
        if (!this.isGameAlive || !this._websocket) {
            throw new Error("The game does not appear to be alive, or a websocket has not been created yet.")
        }
        
        return this._websocket.sendRequest(payload, { requestId: uuidv4() });
    }
    
    public connect(doRetry: boolean = true): Promise<unknown> | undefined {
        // if the game currently isn't alive, there's nothing for us to do.
        if (!this.isGameAlive) {
            console.warn("[XIVDeck - FFXIVLink] Attempted websocket connection while game should be dead.")
            return;
        }
        
        if (this._websocket != null) {
            console.warn("[XIVDeck - FFXIVLink] A connect was called with an already-existing websocket!");
            return;
        }
        
        this._doConnectionRetries = doRetry;
        this._websocket = new WebSocketAsPromisedFaster("ws://localhost:" + this.port + "/xivdeck", {
            packMessage: ((data) => JSON.stringify(data)),
            unpackMessage: ((data) => JSON.parse(data.toString())),
            attachRequestId: FFXIVPluginLink._attachContext,
            extractRequestId: FFXIVPluginLink._retrieveContext
        });
        
        this._websocket.onOpen.addListener(() => {
            this.send(new InitOpcode("0.0.1")); // todo: figure out some way to read the version dynamically
            this.emit("_wsOpened", null);
            
            this.isGameAlive = true;
        });
        
        this._websocket.onUnpackedMessage.addListener(this._onWSMessage.bind(this))
        this._websocket.onClose.addListener(this._onWSClose.bind(this));

        return this._websocket.open();
    }
    
    private _onWSMessage(data: any): void {
        console.debug("[XIVDeck - FFXIVPluginLink] Got message from websocket", data);
        
        if (!data.hasOwnProperty('messageType')) {
            console.warn("[XIVDeck - FFXIVPluginLink] Malformed message from XIV Plugin, missing messageType", data);
            return
        }
        
        this.emit(data.messageType, data);
    }
    
    private _onWSClose(): void {
        console.debug("[XIVDeck - FFXIVPluginLink] Connection to WebSocket server lost!")
        this.emit("_wsClosed", {});
        this._websocket = null;
        
        if (!this._doConnectionRetries || !this.isGameAlive) return;
        
        // retry a connection after 500 milliseconds, specifying connection retry logic.
        // because of how websocket closes work, a failed connection will emit a close and will hit this repeatedly.
        setTimeout(this.connect.bind(this), 500);
    }

    /**
     * Immediately close the websocket, disabling further attempts at connection retry.
     */
    public close(): void {
        this._doConnectionRetries = false;
        
        if (this._websocket != null) this._websocket.close();
    }
    
    public isReady(): boolean {
        return ((this._websocket != null) && (this._websocket.isOpened));
    }

    public on(name: string, fn: Function): Function | undefined {
        return FFXIVPluginLink.eventManager.on(name, fn);
    }
    
    public nowAndOn(eventName: string, fn: Function): Function | undefined {
        if (this.isReady() && this.isGameAlive) {
            fn();
        }
        
        return this.on(eventName, fn);
    }
    
    private emit(name: string, data: any): void {
        FFXIVPluginLink.eventManager.emit(name, data);
    }
}
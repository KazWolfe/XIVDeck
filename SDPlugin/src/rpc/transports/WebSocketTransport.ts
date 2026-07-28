import WebSocket from "ws";
import {RpcTransport} from "./RpcTransport";
import type {WebSocketTransportConfig} from "../../settings/types/TransportConfig";

export const DEFAULT_WS_PORT: number = 37984;

export class WebSocketTransport implements RpcTransport {
    readonly label: string;

    private _socket?: WebSocket;
    private _messageHandler?: (message: Uint8Array) => void;
    private _closeHandler?: (err?: Error) => void;

    constructor(private readonly hostname: string, private readonly port: number) {
        this.label = `ws:${hostname}:${port}`;
    }

    static fromConfig(config: WebSocketTransportConfig): WebSocketTransport {
        return new WebSocketTransport(config.hostname || "localhost", config.port || DEFAULT_WS_PORT);
    }

    connect(): Promise<void> {
        return new Promise((resolve, reject) => {
            const socket = new WebSocket(`ws://${this.hostname}:${this.port}/`);
            this._socket = socket;

            let connected = false;
            socket.once("open", () => {
                connected = true;
                resolve();
            });
            socket.once("error", err => {
                if (!connected) reject(err);
            });

            socket.on("message", data => this._messageHandler?.(data as Buffer));
            socket.on("close", () => {
                if (connected) this._closeHandler?.();
            });
            socket.on("error", err => {
                if (connected) this._closeHandler?.(err as Error);
            });
        });
    }

    close(): void {
        if (this._socket?.readyState === WebSocket.OPEN) {
            this._socket.close(1000, "SDPlugin shutting down.");
        } else {
            this._socket?.terminate();
        }
        this._socket = undefined;
    }

    send(message: Uint8Array): void {
        this._socket?.send(message);
    }

    onMessage(handler: (message: Uint8Array) => void): void {
        this._messageHandler = handler;
    }

    onClose(handler: (err?: Error) => void): void {
        this._closeHandler = handler;
    }
}

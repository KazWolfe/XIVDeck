import WebSocket from "ws";
import {IRpcTransport} from "./IRpcTransport";
import {EventSource, IEvent} from "#/util/EventSource";

export class WebSocketTransport implements IRpcTransport {
    private readonly _message = new EventSource<[Uint8Array]>();
    private readonly _closed = new EventSource<[Error | undefined]>();
    private _socket?: WebSocket;

    public readonly message: IEvent<[Uint8Array]> = this._message;
    public readonly closed: IEvent<[Error | undefined]> = this._closed;

    public constructor(private readonly hostname: string, private readonly port: number) {
    }

    public connect(): Promise<void> {
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

            socket.on("message", data => this._message.emit(data as Buffer));
            socket.on("close", () => {
                if (connected) this._closed.emit(undefined);
            });
            socket.on("error", err => {
                if (connected) this._closed.emit(err as Error);
            });
        });
    }

    public close(): void {
        if (this._socket?.readyState === WebSocket.OPEN) {
            this._socket.close(1000, "SDPlugin shutting down.");
        } else {
            this._socket?.terminate();
        }
        this._socket = undefined;
    }

    public send(message: Uint8Array): void {
        this._socket?.send(message);
    }
}

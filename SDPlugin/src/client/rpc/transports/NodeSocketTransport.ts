import * as net from "node:net";
import {IRpcTransport} from "./IRpcTransport";
import {encodeFrame, FrameDecoder} from "./protocol/LengthPrefixFraming";
import {EventSource, IEvent} from "#/util/EventSource";

export abstract class NodeSocketTransport implements IRpcTransport {
    private readonly _message = new EventSource<[Uint8Array]>();
    private readonly _closed = new EventSource<[Error | undefined]>();
    private readonly _decoder: FrameDecoder;
    private _socket?: net.Socket;

    public readonly message: IEvent<[Uint8Array]> = this._message;
    public readonly closed: IEvent<[Error | undefined]> = this._closed;

    protected constructor(private readonly target: string) {
        this._decoder = new FrameDecoder(this.onFrame.bind(this));
    }

    public connect(): Promise<void> {
        return new Promise((resolve, reject) => {
            const socket = net.connect(this.target);
            this._socket = socket;

            let connected = false;
            socket.once("connect", () => {
                connected = true;
                resolve();
            });
            socket.once("error", err => {
                if (!connected) reject(err);
            });

            socket.on("data", (chunk: Buffer) => {
                try {
                    this._decoder.push(chunk);
                } catch (err) {
                    // sacrifice the connection instead of letting the exception go out
                    socket.destroy(err instanceof Error ? err : new Error(String(err)));
                }
            });
            socket.on("close", () => {
                if (connected) this._closed.emit(undefined);
            });
            socket.on("error", err => {
                if (connected) this._closed.emit(err);
            });
        });
    }

    public close(): void {
        this._socket?.destroy();
        this._socket = undefined;
    }

    public send(message: Uint8Array): void {
        this._socket?.write(encodeFrame(message));
    }

    private onFrame(message: Uint8Array): void {
        this._message.emit(message);
    }
}

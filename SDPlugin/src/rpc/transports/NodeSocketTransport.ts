import * as net from "node:net";
import {RpcTransport} from "./RpcTransport";
import {encodeFrame, FrameDecoder} from "./protocol/LengthPrefixFraming";

export abstract class NodeSocketTransport implements RpcTransport {
    private _socket?: net.Socket;
    private readonly _decoder: FrameDecoder;
    private _messageHandler?: (message: Uint8Array) => void;
    private _closeHandler?: (err?: Error) => void;

    protected constructor(readonly label: string, private readonly target: string) {
        this._decoder = new FrameDecoder(message => this._messageHandler?.(message));
    }

    connect(): Promise<void> {
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
                if (connected) this._closeHandler?.();
            });
            socket.on("error", err => {
                if (connected) this._closeHandler?.(err);
            });
        });
    }

    close(): void {
        this._socket?.destroy();
        this._socket = undefined;
    }

    send(message: Uint8Array): void {
        this._socket?.write(encodeFrame(message));
    }

    onMessage(handler: (message: Uint8Array) => void): void {
        this._messageHandler = handler;
    }

    onClose(handler: (err?: Error) => void): void {
        this._closeHandler = handler;
    }
}

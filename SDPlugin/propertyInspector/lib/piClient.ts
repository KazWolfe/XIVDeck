import {JsonObject} from "@elgato/utils";
import {StreamDeckSocket} from "./StreamDeckSocket";
import {KeyedEventEmitter} from "#/util/KeyedEventEmitter";
import type {PiPushEvents} from "#/client/rpc/messages/PiPushEvents";

interface PiRequestEnvelope {
    id: number;
    command: string;
    params?: unknown;
}

interface PiResponseEnvelope {
    id: number;
    result?: unknown;
    error?: string;
}

type PiPushEnvelope = { [K in keyof PiPushEvents]: { event: K; data: PiPushEvents[K] } }[keyof PiPushEvents];

type PushEventMap = { [K in keyof PiPushEvents]: [data: PiPushEvents[K]] };

const RESEND_INTERVAL_MS = 1000;
const MAX_SENDS = 3;
const REQUEST_TIMEOUT_MS = 10_000;

export class PiClient {
    private _nextId = 1;
    private readonly _pending = new Map<number, { resolve: (v: unknown) => void; reject: (err: Error) => void }>();
    private readonly _push = new KeyedEventEmitter<PushEventMap>();

    constructor(private readonly socket: StreamDeckSocket<JsonObject>) {
        socket.onSendToPropertyInspector(raw => this._onMessage(raw as unknown as PiResponseEnvelope | PiPushEnvelope));
    }

    request<T = unknown>(command: string, params?: unknown): Promise<T> {
        return new Promise<T>((resolve, reject) => {
            const ids: number[] = [];
            let sends = 0;

            const settle = () => {
                clearInterval(resendTimer);
                clearTimeout(timeoutTimer);
                for (const id of ids) this._pending.delete(id);
            };

            const pending = {
                resolve: (value: unknown) => {
                    settle();
                    resolve(value as T);
                },
                reject: (err: Error) => {
                    settle();
                    reject(err);
                },
            };

            const send = () => {
                const id = this._nextId++;
                ids.push(id);
                this._pending.set(id, pending);
                this.socket.sendToPlugin({id, command, params} satisfies PiRequestEnvelope as unknown as JsonObject);

                if (++sends >= MAX_SENDS) clearInterval(resendTimer);
            };

            const resendTimer = setInterval(send, RESEND_INTERVAL_MS);
            const timeoutTimer = setTimeout(
                () => pending.reject(new Error(`Request "${command}" timed out.`)),
                REQUEST_TIMEOUT_MS,
            );

            send();
        });
    }

    onPush<K extends keyof PiPushEvents>(event: K, listener: (data: PiPushEvents[K]) => void): () => void {
        const handler = (data: PiPushEvents[K]): void => listener(data);
        this._push.subscribe(event, handler);

        return () => this._push.unsubscribe(event, handler);
    }

    private _onMessage(message: PiResponseEnvelope | PiPushEnvelope): void {
        if ("event" in message) {
            this._emitPush(message);
            return;
        }

        if (message.id == null || !this._pending.has(message.id)) return;

        const pending = this._pending.get(message.id)!;
        this._pending.delete(message.id);

        if (message.error) {
            pending.reject(new Error(message.error));
        } else {
            pending.resolve(message.result);
        }
    }

    // Generic over the event, so TS keeps each envelope's `event` and `data` paired instead of as two unrelated unions.
    private _emitPush<K extends keyof PiPushEvents>(envelope: { event: K; data: PiPushEvents[K] }): void {
        // `[PiPushEvents[K]]` is `PushEventMap[K]` by definition; TS just can't resolve the mapped type for a generic K.
        const args = [envelope.data] as PushEventMap[K];
        this._push.emit(envelope.event, ...args);
    }
}

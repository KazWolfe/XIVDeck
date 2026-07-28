import {decode, encode} from "@msgpack/msgpack";
import {EventEmitter} from "../util/EventEmitter";
import {RpcTransport} from "./transports/RpcTransport";
import {TransportResolver} from "./TransportDiscovery";
import {ServerHello} from "./messages/Connection";
import {RpcNotificationMap, RpcRequestMap} from "./RpcContract";
import {IconProvider} from "./IconProvider";

const CLIENT_VERSION = __BUILD_VERSION__;

const RETRY_INTERVAL_MS = 1_000;

interface ClientEventMap extends RpcNotificationMap {
    _ready: [ServerHello];
    _closed: [];
}

export class RpcRequestError extends Error {
    constructor(public readonly code: number, message: string, public readonly data?: unknown) {
        super(message);
    }
}

interface PendingRequest {
    resolve: (value: unknown) => void;
    reject: (err: Error) => void;
}

export class XivDeckClient {
    private _emitter = new EventEmitter<ClientEventMap>();

    // Must be initialized after _emitter, as it subscribes to _closed.
    public readonly icons = new IconProvider(this);
    private _transport?: RpcTransport;
    private _pending = new Map<number, PendingRequest>();
    private _nextId = 1;
    private _ready = false;
    private _resolveTransport?: TransportResolver;
    private _doRetry = true;
    private _gameVersion?: string;
    private _connecting?: Promise<void>;
    private _retryTimer?: ReturnType<typeof setTimeout>;

    // Bumped on shutdown so that connection attempts started beforehand discard their result.
    private _generation = 0;

    public isReady(): boolean {
        return this._ready;
    }

    public get gameVersion(): string | undefined {
        return this._gameVersion;
    }

    public get transportLabel(): string | undefined {
        return this._transport?.label;
    }

    public on<K extends keyof ClientEventMap>(name: K, fn: (...args: ClientEventMap[K]) => void): () => void {
        return this._emitter.on(name, fn);
    }


    public connect(resolveTransport: TransportResolver, doRetry = true): Promise<void> {
        this._resolveTransport = resolveTransport;
        this._doRetry = doRetry;
        this._cancelRetry();

        if (this._transport || this._connecting) {
            console.warn("[XivDeckClient] connect() called while already connected/connecting.");
            return this._connecting ?? Promise.resolve();
        }

        const attempt = this._connect(resolveTransport).finally(() => {
            if (this._connecting === attempt) this._connecting = undefined;
        });
        this._connecting = attempt;
        return attempt;
    }

    private async _connect(resolveTransport: TransportResolver): Promise<void> {
        const generation = this._generation;
        const candidates = resolveTransport();

        if (candidates.length === 0) {
            console.warn("[XivDeckClient] No transport available to connect with.");
            this._scheduleRetry();
            return;
        }

        // try actually connecting to see if we're alive.
        const transport = await this._dial(candidates, generation);

        if (!transport) {
            if (generation === this._generation) this._scheduleRetry();
            return;
        }

        this._transport = transport;
        console.debug(`[XivDeckClient] Connected via ${transport.label}, sending handshake.`);

        try {
            const hello = await this.request("Connection.Initialize", {
                clientHello: {
                    clientVersion: CLIENT_VERSION,
                    clientType: "Plugin",
                },
            });

            if (!hello.accepted) {
                console.warn("[XivDeckClient] Server rejected handshake (incompatible version).");
                this.shutdown();
                return;
            }

            this._ready = true;
            this._gameVersion = hello.ffxivPluginVersion;
            this._emitter.emit("_ready", hello);
        } catch (err) {
            console.warn("[XivDeckClient] Handshake failed:", err);

            if (this._transport === transport) this.shutdown();
        }
    }

    private async _dial(candidates: RpcTransport[], generation: number): Promise<RpcTransport | undefined> {
        const failed: string[] = [];

        for (const transport of candidates) {
            if (generation !== this._generation) return undefined;

            transport.onMessage(message => {
                if (this._transport === transport) this._onMessage(message);
            });
            transport.onClose(err => {
                if (this._transport === transport) this._onClose(err);
            });

            try {
                await transport.connect();
            } catch (err) {
                console.debug(`[XivDeckClient] Failed to connect via ${transport.label}:`, err);

                transport.close();
                failed.push(transport.label);
                continue;
            }

            if (generation !== this._generation) {
                console.debug(`[XivDeckClient] Shut down while connecting via ${transport.label}, discarding connection.`);
                transport.close();
                return undefined;
            }

            return transport;
        }

        console.warn(`[XivDeckClient] No transport answered (tried: ${failed.join(", ")}).`);
        return undefined;
    }

    public shutdown(): void {
        this._doRetry = false;
        this._generation++;
        this._connecting = undefined;
        this._cancelRetry();

        const transport = this._transport;
        if (transport) {
            this._detach();
            transport.close();
        }
    }

    public gracefulClose(): void {
        this._transport?.close();
    }

    public request<K extends keyof RpcRequestMap>(
        method: K,
        ...args: RpcRequestMap[K]["params"] extends undefined ? [] : [RpcRequestMap[K]["params"]]
    ): Promise<RpcRequestMap[K]["result"]> {
        if (!this._transport) {
            return Promise.reject(new Error("Not connected."));
        }

        const id = this._nextId++;
        const envelope: Record<string, unknown> = {jsonrpc: "2.0", id, method};
        if (args[0] !== undefined) envelope.params = args[0];
        const payload = encode(envelope);

        return new Promise((resolve, reject) => {
            this._pending.set(id, {resolve: resolve as (v: unknown) => void, reject});
            this._transport!.send(payload);
        });
    }

    private _onMessage(raw: Uint8Array): void {
        let message: any;
        try {
            message = decode(raw);
        } catch (err) {
            console.warn("[XivDeckClient] Got undecodable message:", raw, err);
            return;
        }

        if (message.id !== undefined && message.id !== null && this._pending.has(message.id)) {
            const pending = this._pending.get(message.id)!;
            this._pending.delete(message.id);

            if (message.error) {
                pending.reject(new RpcRequestError(message.error.code, message.error.message, message.error.data));
            } else {
                pending.resolve(message.result);
            }
            return;
        }

        if (message.method) {
            const args: unknown[] = Array.isArray(message.params) ? message.params : message.params !== undefined ? [message.params] : [];
            const method = message.method as keyof RpcNotificationMap;

            this._emitter.emit(method, ...(args as ClientEventMap[typeof method]));
            return;
        }

        console.debug("[XivDeckClient] Got unexpected message shape:", message);
    }

    private _onClose(err?: Error): void {
        console.debug("[XivDeckClient] Connection closed.", err);

        this._detach();
        this._scheduleRetry();
    }

    private _detach(): void {
        this._transport = undefined;
        this._ready = false;
        this._gameVersion = undefined;
        this._emitter.emit("_closed");

        for (const pending of this._pending.values()) {
            pending.reject(new Error("Connection closed."));
        }
        this._pending.clear();
    }

    private _scheduleRetry(): void {
        if (!this._doRetry || !this._resolveTransport || this._retryTimer) return;

        this._retryTimer = setTimeout(() => {
            this._retryTimer = undefined;
            void this.connect(this._resolveTransport!, true);
        }, RETRY_INTERVAL_MS);
    }

    private _cancelRetry(): void {
        clearTimeout(this._retryTimer);
        this._retryTimer = undefined;
    }
}

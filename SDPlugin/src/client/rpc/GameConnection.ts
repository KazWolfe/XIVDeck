import {decode, encode} from "@msgpack/msgpack";
import streamDeck from "@elgato/streamdeck";
import {EventHandler, EventSource, IEvent} from "#/util/EventSource";
import {KeyedEventEmitter} from "#/util/KeyedEventEmitter";
import {IRpcTransport} from "#/client/rpc/transports/IRpcTransport";
import {NamedPipeTransport} from "#/client/rpc/transports/NamedPipeTransport";
import {UnixSocketTransport} from "#/client/rpc/transports/UnixSocketTransport";
import {WebSocketTransport} from "#/client/rpc/transports/WebSocketTransport";
import {DEFAULT_WS_PORT, Endpoint, Endpoints} from "#/settings/types/TransportSettings";
import {RpcNotificationMap, RpcRequestMap} from "#/client/rpc/RpcContract";
import {ServerHello} from "#/client/rpc/messages/Connection";

export type RpcParams<K extends keyof RpcRequestMap> =
    RpcRequestMap[K]["params"] extends undefined ? [] : [RpcRequestMap[K]["params"]];

export type RpcResult<K extends keyof RpcRequestMap> = RpcRequestMap[K]["result"];

/** Raised by the game when a request fails; `code` is usually an {@link RpcErrorCode}. */
export class RpcRequestError extends Error {
    public constructor(public readonly code: number, message: string, public readonly data?: unknown) {
        super(message);
    }
}

/** Raised when a request is made without a ready connection. Expected while the game is not running. */
export class NoConnectionError extends Error {
}

interface IPendingRequest {
    resolve(value: unknown): void;

    reject(err: Error): void;
}

interface IRpcErrorBody {
    code: number;
    message: string;
    data?: unknown;
}

function isRecord(value: unknown): value is Record<string, unknown> {
    return typeof value === "object" && value !== null && !Array.isArray(value);
}

function isRpcErrorBody(value: unknown): value is IRpcErrorBody {
    return isRecord(value) && typeof value.code === "number" && typeof value.message === "string";
}

/**
 * A JSON-RPC (msgpack) session with the XIVDeck server at one endpoint, over a new transport each time it dials.
 * Client-scoped; constructed by {@link ClientManager} with its endpoint and bound into the client scope as an instance.
 *
 * Once {@link start}ed, keeps itself connected for its whole lifetime: it redials a second after a failed attempt or a
 * dropped connection, until disposed. Whether it exists at all (only while a game is running) is
 * {@link ClientManager}'s job.
 */
export class GameConnection implements Disposable {
    private static readonly CLIENT_VERSION: string = __BUILD_VERSION__;
    private static readonly RETRY_INTERVAL_MS = 1_000;

    private readonly _ready = new EventSource<[ServerHello]>();
    private readonly _closed = new EventSource();
    private readonly _notifications = new KeyedEventEmitter<RpcNotificationMap>();
    private readonly _pending = new Map<string, IPendingRequest>();

    private _transport?: IRpcTransport;
    private _isReady = false;
    private _isOpening = false;
    private _isDisposed = false;
    private _reportedUnreachable = false;
    private _retryTimer?: NodeJS.Timeout;

    /** The handshake completed; requests may now be made. */
    public readonly ready: IEvent<[ServerHello]> = this._ready;

    /** A previously opened transport went away, whether by {@link close} or by the remote end. */
    public readonly closed: IEvent = this._closed;

    public get isReady(): boolean {
        return this._isReady;
    }

    public constructor(public readonly endpoint: Endpoint) {
    }

    /** Starts dialing, and keeps redialing until connected and whenever the connection drops. */
    public start(): void {
        this.attempt();
    }

    /**
     * Make an RPC request to the game.
     * @param method The RPC method name to call.
     * @param params The parameters to pass to the RPC method.
     * @throws NoConnectionError when a connection has not been established.
     * @throws RpcRequestError when the game rejects the request.
     */
    public request<K extends keyof RpcRequestMap>(method: K, ...params: RpcParams<K>): Promise<RpcResult<K>> {
        if (!this._isReady) {
            return Promise.reject(new NoConnectionError("Not connected."));
        }

        return this.sendRequest(method, ...params);
    }

    /**
     * Subscribe to a notification from the game.
     * @param name The RPC notification name to listen for.
     * @param handler The handler to call when the notification is received.
     */
    public subscribe<K extends keyof RpcNotificationMap>(name: K, handler: EventHandler<RpcNotificationMap[K]>): void {
        this._notifications.subscribe(name, handler);
    }

    /**
     * Unsubscribe from a prior-subscribed notification.
     * @param name The RPC notification name to stop listening for.
     * @param handler The handler to stop calling when the notification is received.
     */
    public unsubscribe<K extends keyof RpcNotificationMap>(name: K, handler: EventHandler<RpcNotificationMap[K]>): void {
        this._notifications.unsubscribe(name, handler);
    }

    public [Symbol.dispose](): void {
        this._isDisposed = true;
        clearTimeout(this._retryTimer);
        this._retryTimer = undefined;
        this.close();
        this._ready.clear();
        this._closed.clear();
        this._notifications.clear();
    }

    /** Dials unless already connected, connecting, or disposed; schedules a retry if the attempt fails. */
    private attempt(): void {
        if (this._isDisposed || this._isReady || this._isOpening) return;

        this.open().catch((err: unknown) => {
            streamDeck.logger.error(`${this.logTag} Unexpected failure dialing:`, err);
            this.scheduleRetry();
        });
    }

    /**
     * Dials the endpoint over a new transport, then performs the `Connection.Initialize` handshake. Schedules a retry
     * unless it ends up ready (or this connection was disposed meanwhile).
     */
    private async open(): Promise<void> {
        this._isOpening = true;
        const transport = GameConnection.createTransport(this.endpoint);

        let opened = false;
        try {
            if (await this.dial(transport)) {
                this.attach(transport);
                opened = await this.handshake(transport);
            }
        } catch (err) {
            streamDeck.logger.warn(`${this.logTag} Unexpected failure while opening:`, err);
            this.close();
        } finally {
            this._isOpening = false;
        }

        if (!opened) {
            this.scheduleRetry();
        }
    }

    private scheduleRetry(): void {
        if (this._isDisposed || this._retryTimer) return;

        this._retryTimer = setTimeout(this.onRetryTimer.bind(this), GameConnection.RETRY_INTERVAL_MS);
    }

    private onRetryTimer(): void {
        this._retryTimer = undefined;
        this.attempt();
    }

    private get logTag(): string {
        return `[GameConnection:${Endpoints.keyOf(this.endpoint)}]`;
    }

    private static createTransport(endpoint: Endpoint): IRpcTransport {
        switch (endpoint.kind) {
            case "ws":
                return new WebSocketTransport(endpoint.host, endpoint.port ?? DEFAULT_WS_PORT);
            case "pipe":
                return new NamedPipeTransport(endpoint.name);
            case "uds":
                return new UnixSocketTransport(endpoint.path);
        }
    }

    private async dial(transport: IRpcTransport): Promise<boolean> {
        try {
            await transport.connect();
        } catch (err) {
            streamDeck.logger.debug(`${this.logTag} Failed to connect:`, err);
            this.reportUnreachable();
            transport.close();
            return false;
        }

        if (this._isDisposed) {
            streamDeck.logger.debug(`${this.logTag} Disposed while connecting, discarding.`);
            transport.close();
            return false;
        }

        return true;
    }

    /** Logs once per outage; retries continue quietly until the endpoint answers. */
    private reportUnreachable(): void {
        if (this._reportedUnreachable) return;
        this._reportedUnreachable = true;

        streamDeck.logger.info(`${this.logTag} Not reachable yet, retrying.`);
    }

    private async handshake(transport: IRpcTransport): Promise<boolean> {
        streamDeck.logger.debug(`${this.logTag} Connected, sending handshake.`);

        let hello: ServerHello;
        try {
            hello = await this.sendRequest("Connection.Initialize", {
                clientHello: {
                    clientVersion: GameConnection.CLIENT_VERSION,
                    clientType: "Plugin",
                },
            });
        } catch (err) {
            streamDeck.logger.warn(`${this.logTag} Handshake failed:`, err);
            if (this._transport === transport) this.close();
            return false;
        }

        // closed (or disposed) while waiting for the reply.
        if (this._transport !== transport) return false;

        if (!hello.accepted) {
            streamDeck.logger.warn(
                `${this.logTag} Server rejected handshake (incompatible version).`);
            this.close();
            return false;
        }

        this._isReady = true;
        this._reportedUnreachable = false;
        this._ready.emit(hello);
        return true;
    }

    private attach(transport: IRpcTransport): void {
        this._transport = transport;
        transport.message.add(this.onTransportMessage);
        transport.closed.add(this.onTransportClosed);
    }

    /** Closes the live transport, if any. Pending requests reject; {@link closed} fires if a transport was open. */
    private close(): void {
        const transport = this._transport;
        if (!transport) return;

        transport.message.remove(this.onTransportMessage);
        transport.closed.remove(this.onTransportClosed);
        this._transport = undefined;
        this._isReady = false;

        transport.close();

        const pending = [...this._pending.values()];
        this._pending.clear();
        for (const request of pending) {
            request.reject(new NoConnectionError("Connection closed."));
        }

        this._closed.emit();
    }

    private sendRequest<K extends keyof RpcRequestMap>(method: K, ...params: RpcParams<K>): Promise<RpcResult<K>> {
        const transport = this._transport;
        if (!transport) {
            return Promise.reject(new NoConnectionError("Not connected."));
        }

        const id = crypto.randomUUID();
        const envelope: Record<string, unknown> = {jsonrpc: "2.0", id, method};
        if (params[0] !== undefined) envelope.params = params[0];
        const payload = encode(envelope);

        return new Promise<RpcResult<K>>((resolve, reject) => {
            this._pending.set(id, {resolve: resolve as (value: unknown) => void, reject});

            try {
                transport.send(payload);
            } catch (err) {
                this._pending.delete(id);
                reject(err instanceof Error ? err : new Error(String(err)));
            }
        });
    }

    private readonly onTransportMessage = (raw: Uint8Array): void => {
        let message: unknown;
        try {
            message = decode(raw);
        } catch (err) {
            streamDeck.logger.warn(`${this.logTag} Got undecodable message:`, err);
            return;
        }

        if (!isRecord(message)) {
            streamDeck.logger.debug(`${this.logTag} Got unexpected message shape:`, message);
            return;
        }

        if (typeof message.id === "string" && this._pending.has(message.id)) {
            this.completeRequest(message.id, message);
            return;
        }

        if (typeof message.method === "string") {
            let args: unknown[] = [];
            if (Array.isArray(message.params)) {
                args = message.params;
            } else if (message.params !== undefined) {
                args = [message.params];
            }

            this._notifications.emit(
                message.method as keyof RpcNotificationMap,
                ...(args as RpcNotificationMap[keyof RpcNotificationMap]),
            );
            return;
        }

        streamDeck.logger.debug(`${this.logTag} Got unexpected message shape:`, message);
    };

    private readonly onTransportClosed = (err: Error | undefined): void => {
        streamDeck.logger.debug(`${this.logTag} Transport closed.`, err);
        this.close();
        this.scheduleRetry();
    };

    private completeRequest(id: string, message: Record<string, unknown>): void {
        const pending = this._pending.get(id);
        if (!pending) return;
        this._pending.delete(id);

        if (message.error !== undefined && message.error !== null) {
            if (isRpcErrorBody(message.error)) {
                pending.reject(new RpcRequestError(message.error.code, message.error.message, message.error.data));
            } else {
                pending.reject(new RpcRequestError(-1, "Malformed error response.", message.error));
            }
            return;
        }

        pending.resolve(message.result);
    }
}

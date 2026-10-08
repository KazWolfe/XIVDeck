import {IEvent} from "#/util/EventSource";

/**
 * A transport is an individual *instance* of a connection to the game's RPC server (functionally a stream/channel).
 * A new transport is opened for every connection attempt, and closed upon death.
 *
 * See {@link GameConnection} for the actual RPC "session" layer that handles our protocol.
 */
export interface IRpcTransport {
    readonly message: IEvent<[Uint8Array]>;
    readonly closed: IEvent<[Error | undefined]>;

    connect(): Promise<void>;
    close(): void;
    send(message: Uint8Array): void;
}

import {XivDeckClient} from "./XivDeckClient";
import {EventEmitter} from "../util/EventEmitter";

interface ClientRegistryEvents {
    clientChanged: [transportId: string];
}

/**
 * A provider for client management. Used for multiboxing or parallel sessions down the line.
 */
export interface ClientProvider {
    getClient(transportId: string): XivDeckClient;

    getAllClients(): readonly XivDeckClient[];

    /** Fires whenever the client registered for a transport ID is added or replaced. */
    onClientChanged(handler: (transportId: string) => void): () => void;

    addClient(transportId: string, client: XivDeckClient): void;
}

export class ClientRegistry implements ClientProvider {
    private readonly _clients = new Map<string, XivDeckClient>();
    private readonly _events = new EventEmitter<ClientRegistryEvents>();

    getClient(transportId: string): XivDeckClient {
        const client = this._clients.get(transportId);
        if (!client) {
            throw new Error(`No XivDeckClient has been registered for transport "${transportId}".`);
        }

        return client;
    }

    getAllClients(): readonly XivDeckClient[] {
        return [...this._clients.values()];
    }

    onClientChanged(handler: (transportId: string) => void): () => void {
        return this._events.on("clientChanged", handler);
    }

    addClient(transportId: string, client: XivDeckClient): void {
        this._clients.set(transportId, client);
        this._events.emit("clientChanged", transportId);
    }
}

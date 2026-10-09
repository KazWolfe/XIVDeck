import {injectable} from "inversify";
import {ClientManager} from "#/client/ClientManager";
import {Client} from "#/client/Client";
import {NoConnectionError, RpcParams, RpcResult} from "#/client/rpc/GameConnection";
import {RpcRequestMap} from "#/client/rpc/RpcContract";
import {ConnectionState, DISCONNECTED_STATE} from "#/client/ConnectionState";
import {EventSource, IEvent} from "#/util/EventSource";
import {IProxyService} from "./IProxyService";

/**
 * A control-scoped service that acts as the link between the owning control and the game client it's bound to. This
 * manages basically everything related to the control's communication lifecycle.
 *
 * Child services (see `IProxyService`) are also managed by this service, thereby allowing controls to basically
 * completely ignore any game client workings.
 */
@injectable()
export class ClientProxy implements Disposable {
    private readonly _availabilityChanged = new EventSource();
    private readonly _services: IProxyService[] = [];

    /** The ID of the game connection we want to connect to, or `null` to follow the default. */
    private _targetConnectionId: string | null = null;
    private _client: Client | undefined;

    /**
     * A signal that the connection this control uses has changed. This generally means that the connection to the game
     * was lost or established, or that the control switched to a different client.
     */
    public readonly availabilityChanged: IEvent = this._availabilityChanged;

    public constructor(private readonly manager: ClientManager) {
        manager.beforeClientRemoval.add(this.onManagerChanged);
        manager.changed.add(this.onManagerChanged);

        // Connect now; services will attach as they come online.
        this.synchronizeClient();
    }

    public get isAvailable(): boolean {
        return this._client?.isConnected ?? false;
    }

    public get connectionState(): ConnectionState {
        const client = this._client;
        if (!client?.isConnected) {
            return DISCONNECTED_STATE;
        }

        return {
            endpointKind: client.connection.endpoint.kind,
            gameVersion: client.gameVersion ?? null,
        };
    }

    public async request<K extends keyof RpcRequestMap>(method: K, ...params: RpcParams<K>): Promise<RpcResult<K>> {
        return await this.requireClient().connection.request(method, ...params);
    }

    /** Helper method to get an icon directly (Law of Demeter). */
    public async getIcon(iconId: number): Promise<string> {
        return await this.requireClient().icons.getBase64(iconId);
    }

    /**
     * Adds a service to be managed by this proxy.
     * Note that services cannot be removed once registered (for now?).
     */
    public registerService(service: IProxyService): void {
        this._services.push(service);

        if (this._client) {
            service.attach(this._client);
        }
    }

    // noinspection JSUnusedGlobalSymbols - future work
    /**
     * Choose the client that this proxy (and, by proxy, the control) connects to.
     * @param connectionId The saved connection's ID, or `null` to follow the globally configured client.
     */
    public setTargetConnection(connectionId: string | null): void {
        this._targetConnectionId = connectionId;
        this.synchronizeClient();
    }

    public [Symbol.dispose](): void {
        this.detachClient();
        this.manager.beforeClientRemoval.remove(this.onManagerChanged);
        this.manager.changed.remove(this.onManagerChanged);

        this._services.length = 0;
        this._availabilityChanged.clear();
    }

    private readonly onManagerChanged = (): void => {
        this.synchronizeClient();
    };

    private readonly onConnectionChanged = (): void => {
        this._availabilityChanged.emit();
    };

    private synchronizeClient(): void {
        const next = this.manager.getClient(this._targetConnectionId);
        if (next === this._client) {
            return;
        }

        this.detachClient();
        if (next) {
            this.attachClient(next);
        }

        this._availabilityChanged.emit();
    }

    private requireClient(): Client {
        if (!this._client) {
            throw new NoConnectionError("No game client is available for this control.");
        }

        return this._client;
    }

    private attachClient(client: Client): void {
        this._client = client;
        client.connectionChanged.add(this.onConnectionChanged);

        for (const service of this._services) {
            service.attach(client);
        }
    }

    private detachClient(): void {
        const client = this._client;
        if (!client) {
            return;
        }

        for (const service of [...this._services].reverse()) {
            service.detach(client);
        }

        client.connectionChanged.remove(this.onConnectionChanged);
        this._client = undefined;
    }
}

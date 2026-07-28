import streamDeck, {Action} from "@elgato/streamdeck";
import {JsonObject} from "@elgato/utils";
import {SettingsGateway} from "../settings/SettingsGateway";
import {RpcRequestError, XivDeckClient} from "../rpc/XivDeckClient";
import {ClientProvider} from "../rpc/ClientProvider";
import {RpcNotificationMap} from "../rpc/RpcContract";
import {DEFAULT_TRANSPORT_ID} from "../settings/GlobalSettings";
import {isContractError} from "../rpc/messages/RpcError";

export abstract class BaseControl<TSettings extends JsonObject, TAction extends Action<TSettings> = Action<TSettings>> {
    protected readonly context: string;

    /** `undefined` until the user has finished configuring this control; see {@link SettingsGateway.complete}. */
    protected settings: TSettings | undefined;

    private readonly _transportId = DEFAULT_TRANSPORT_ID;

    private readonly _clientSubscriptions: ((client: XivDeckClient) => () => void)[] = [];
    private _clientUnsubscribers: (() => void)[] = [];
    private readonly _unsubscribeClientChanged: () => void;

    protected constructor(
        protected readonly action: TAction,
        private readonly clients: ClientProvider,
        protected readonly settingsKind: string
    ) {
        this.context = action.id;

        this._unsubscribeClientChanged = clients.onClientChanged(transportId => {
            if (transportId !== this._transportId) return;

            this._attachClientSubscriptions();
            void this.safeRender();
        });
    }

    protected get activeClient(): XivDeckClient {
        return this.clients.getClient(this._transportId);
    }

    protected onClientEvent<K extends keyof RpcNotificationMap>(
        event: K, handler: (...args: RpcNotificationMap[K]) => void,
    ): void {
        const subscribe = (client: XivDeckClient) => client.on(event, handler);

        this._clientSubscriptions.push(subscribe);
        this._clientUnsubscribers.push(subscribe(this.activeClient));
    }

    private _attachClientSubscriptions(): void {
        this._detachClientSubscriptions();

        const client = this.activeClient;
        this._clientUnsubscribers = this._clientSubscriptions.map(subscribe => subscribe(client));
    }

    private _detachClientSubscriptions(): void {
        for (const unsubscribe of this._clientUnsubscribers) {
            unsubscribe();
        }
        this._clientUnsubscribers = [];
    }

    async loadSettings(raw: unknown, migrate: boolean): Promise<void> {
        const stored = migrate ? SettingsGateway.load<JsonObject>(this.settingsKind, raw) : raw;

        if (migrate && stored !== raw) {
            await this.action.setSettings(stored as TSettings);
        }

        this.settings = SettingsGateway.complete<TSettings>(this.settingsKind, stored);
        await this.safeRender();
    }

    async safeRender(): Promise<void> {
        try {
            await this.render();
        } catch (err) {
            this.logError("rendering", err);
        }
    }

    async handlePiRequest(command: string, _params: unknown): Promise<unknown> {
        throw new Error(`Unsupported PI command: ${command}`);
    }

    cleanup(): void {
        this._unsubscribeClientChanged();
        this._detachClientSubscriptions();
        this._clientSubscriptions.length = 0;
    }

    protected abstract render(): Promise<void>;

    protected logError(activity: string, err: unknown): void {
        if (err instanceof RpcRequestError && isContractError(err.code)) {
            streamDeck.logger.warn(`RPC error ${activity} ${this.settingsKind} (${this.context}):`, err);
        } else {
            streamDeck.logger.error(`Unexpected error ${activity} ${this.settingsKind} (${this.context}):`, err);
        }
    }
}

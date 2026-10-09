import {injectable} from "inversify";
import {ClientProxy} from "./ClientProxy";
import {Client} from "#/client/Client";
import {IHotbarSubscriber} from "#/client/services/HotbarTracker";
import {HotbarSlotRef} from "#/client/rpc/messages/Hotbar";
import {EventSource, IEvent} from "#/util/EventSource";
import {IProxyService} from "./IProxyService";

@injectable()
export class HotbarTrackerProxy implements IProxyService, IHotbarSubscriber {
    private readonly _changed = new EventSource();

    private _slot: HotbarSlotRef | undefined;
    private _client: Client | undefined;

    public readonly changed: IEvent = this._changed;

    public constructor(proxy: ClientProxy) {
        proxy.registerService(this);
    }

    /** Watch `slot`, replacing any previous slot. */
    public watch(slot: HotbarSlotRef): void {
        this._slot = slot;
        this._client?.hotbars.subscribe(this, slot);
    }

    public release(): void {
        this._slot = undefined;
        this._client?.hotbars.unsubscribe(this);
    }

    public onHotbarSlotChanged(): void {
        this._changed.emit();
    }

    public attach(client: Client): void {
        this._client = client;

        if (this._slot) {
            client.hotbars.subscribe(this, this._slot);
        }
    }

    public detach(client: Client): void {
        client.hotbars.unsubscribe(this);
        this._client = undefined;
    }
}

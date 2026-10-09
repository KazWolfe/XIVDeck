import {injectable} from "inversify";
import {ClientProxy} from "./ClientProxy";
import {ControlContext} from "#/control/ControlContext";
import {Client} from "#/client/Client";
import {ICooldownSubscriber} from "#/client/services/CooldownTicker";
import {ActionCooldownDetail, CooldownGroupDetail} from "#/client/rpc/messages/ActionAppearance";
import {EventSource, IEvent} from "#/util/EventSource";
import {IProxyService} from "./IProxyService";

/**
 * Control-scoped proxy for the client-scoped cooldown tracker. Watches one action's cooldowns at a time.
 */
@injectable()
export class CooldownTrackerProxy implements IProxyService, ICooldownSubscriber {
    private readonly _refreshed = new EventSource<[details: ActionCooldownDetail]>();
    private readonly _tick = new EventSource();

    private _watched: ActionCooldownDetail | undefined;
    private _client: Client | undefined;

    public readonly refreshed: IEvent<[details: ActionCooldownDetail]> = this._refreshed;
    public readonly tick: IEvent = this._tick;

    public constructor(proxy: ClientProxy, private readonly context: ControlContext) {
        proxy.registerService(this);
    }

    public subscribe(details: ActionCooldownDetail): void {
        const groups = CooldownTrackerProxy.groupsOf(details);
        this._watched = details;

        this._client?.cooldowns.setTimersFromGroups(groups);
        this._client?.cooldowns.subscribe(this, groups.map(group => group.groupId));
    }

    public unsubscribe(): void {
        this._watched = undefined;
        this._client?.cooldowns.unsubscribe(this);
    }

    public async onCooldownStarted(): Promise<void> {
        const watched = this._watched;
        const client = this._client;
        if (!watched || !client) {
            return;
        }

        try {
            const fresh = await client.connection.request("Cooldown.GetActionCooldown", {
                actionType: watched.actionType, actionId: watched.actionId,
            });

            if (this._watched !== watched) {
                return;
            }

            this._watched = fresh;
            this._refreshed.emit(fresh);
        } catch (err) {
            this.context.logError("refreshing cooldowns for", err);
        }
    }

    public onCooldownTick(): void {
        this._tick.emit();
    }

    public attach(client: Client): void {
        this._client = client;

        if (this._watched) {
            client.cooldowns.subscribe(this, CooldownTrackerProxy.groupsOf(this._watched).map(group => group.groupId));
        }
    }

    public detach(client: Client): void {
        client.cooldowns.unsubscribe(this);
        this._client = undefined;
    }

    private static groupsOf(details: ActionCooldownDetail): CooldownGroupDetail[] {
        return [details.recastGroup, details.rechargeGroup].filter(group => group != null);
    }
}

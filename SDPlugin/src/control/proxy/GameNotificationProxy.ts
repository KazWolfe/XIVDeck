import {injectable} from "inversify";
import {ClientProxy} from "./ClientProxy";
import {IProxyService} from "./IProxyService";
import {Client} from "#/client/Client";
import {EventHandler} from "#/util/EventSource";
import {RpcNotificationMap} from "#/client/rpc/RpcContract";

interface IGenericProxiedNotification {
    subscribe(client: Client): void;
    unsubscribe(client: Client): void;
}

class ProxiedNotification<K extends keyof RpcNotificationMap> implements IGenericProxiedNotification {
    public constructor(private readonly name: K, private readonly handler: EventHandler<RpcNotificationMap[K]>) {
    }

    public subscribe(client: Client): void {
        client.connection.subscribe(this.name, this.handler);
    }

    public unsubscribe(client: Client): void {
        client.connection.unsubscribe(this.name, this.handler);
    }
}

@injectable()
export class GameNotificationProxy implements IProxyService {
    private readonly _registrations: IGenericProxiedNotification[] = [];
    private _client: Client | undefined;

    public constructor(proxy: ClientProxy) {
        proxy.registerService(this);
    }

    public on<K extends keyof RpcNotificationMap>(name: K, handler: EventHandler<RpcNotificationMap[K]>): void {
        const registration = new ProxiedNotification(name, handler);
        this._registrations.push(registration);

        if (this._client) {
            registration.subscribe(this._client);
        }
    }

    public attach(client: Client): void {
        this._client = client;

        for (const registration of this._registrations) {
            registration.subscribe(client);
        }
    }

    public detach(client: Client): void {
        for (const registration of this._registrations) {
            registration.unsubscribe(client);
        }

        this._client = undefined;
    }
}

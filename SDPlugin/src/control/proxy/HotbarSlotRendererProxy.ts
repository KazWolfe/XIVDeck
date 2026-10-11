import {injectable} from "inversify";
import {ClientProxy} from "./ClientProxy";
import {Client} from "#/client/Client";
import {ActionAppearance} from "#/client/rpc/messages/ActionAppearance";
import {IProxyService} from "./IProxyService";

/** Control-scoped proxy for the client-scoped {@link HotbarSlotRenderer}. */
@injectable()
export class HotbarSlotRendererProxy implements IProxyService {
    private _client: Client | undefined;

    public constructor(proxy: ClientProxy) {
        proxy.registerService(this);
    }

    /** Renders a slot with the attached client's renderer, or `undefined` if there's no client. */
    public buildSlot(appearance: ActionAppearance, iconBase64: string,
                     typeIconSvg: string | undefined): string | undefined {
        return this._client?.slotRenderer.buildSlot(appearance, iconBase64, typeIconSvg);
    }

    public attach(client: Client): void {
        this._client = client;
    }

    public detach(): void {
        this._client = undefined;
    }
}

import {injectable} from "inversify";
import {ControlContext} from "#/control/ControlContext";
import {ClientProxy} from "#/control/proxy/ClientProxy";
import {CooldownTrackerProxy} from "#/control/proxy/CooldownTrackerProxy";
import {ActionAppearance, ActionCooldownDetail} from "#/client/rpc/messages/ActionAppearance";
import {HotbarSlotRenderer} from "./HotbarSlotRenderer";

/** Puts a finished slot image (a data URL) wherever the owning control wants it. */
export type SlotPresentHandler = (image: string) => Promise<void>;

interface IShownSlot {
    appearance: ActionAppearance;
    iconBase64: string;
    typeIconSvg: string | undefined;
}

/**
 * Control-scoped hotbar slot display. A control hands it an appearance; from then on the slot keeps the image current
 * on its own (cooldown timers, refreshed cooldown details) until it's shown something else or cleared. It knows
 * nothing about the device: each new image goes to the control's {@link setPresentHandler} handler.
 */
@injectable()
export class VirtualSlotPresenter {
    private _presentHandler: SlotPresentHandler | undefined;
    private _shown: IShownSlot | undefined;
    private _lastImage: string | undefined;

    public constructor(private readonly context: ControlContext, private readonly client: ClientProxy,
                       private readonly renderer: HotbarSlotRenderer, private readonly cooldowns: CooldownTrackerProxy) {
        cooldowns.refreshed.add(this.onCooldownRefreshed.bind(this));
        cooldowns.tick.add(this.onCooldownTick.bind(this));
    }

    /** Sets where images go. Called once, by the owning control's constructor. */
    public setPresentHandler(handler: SlotPresentHandler): void {
        this._presentHandler = handler;
    }

    /**
     * Shows a freshly fetched appearance and tracks its cooldowns.
     * @param appearance What the slot should show.
     * @param typeIconSvg Badge drawn over the icon (e.g. the macro marker), as SVG markup.
     */
    public async setAppearance(appearance: ActionAppearance, typeIconSvg?: string): Promise<void> {
        const iconBase64 = await this.client.getIcon(appearance.iconId);
        this._shown = {appearance, iconBase64, typeIconSvg};

        if (appearance.cooldownDetails) {
            this.cooldowns.subscribe(appearance.cooldownDetails);
        } else {
            this.cooldowns.unsubscribe();
        }

        await this.present();
    }

    /** Stops tracking whatever was shown. The last image stays up. */
    public clear(): void {
        this._shown = undefined;
        this.cooldowns.unsubscribe();
    }

    private async onCooldownRefreshed(details: ActionCooldownDetail): Promise<void> {
        const shown = this._shown;
        if (!shown) {
            return;
        }

        this._shown = {...shown, appearance: {...shown.appearance, cooldownDetails: details}};

        try {
            await this.present();
        } catch (err) {
            this.context.logError("presenting refreshed cooldowns for", err);
        }
    }

    private async onCooldownTick(): Promise<void> {
        try {
            await this.present();
        } catch (err) {
            this.context.logError("presenting a cooldown tick for", err);
        }
    }

    /** Renders the current state as of right now and presents it, unless it's the image last presented. */
    private async present(): Promise<void> {
        const shown = this._shown;
        const handler = this._presentHandler;
        if (!shown || !handler) {
            return;
        }

        const image = this.renderer.buildSlot(shown.appearance, shown.iconBase64, shown.typeIconSvg);
        if (image === this._lastImage) {
            return;
        }

        await handler(image);
        this._lastImage = image;
    }
}

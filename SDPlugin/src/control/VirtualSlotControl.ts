import {JsonObject} from "@elgato/utils";
import streamDeck, {WillAppearEvent} from "@elgato/streamdeck";
import {InteractiveControl} from "./InteractiveControl";
import {ClientProvider} from "../rpc/ClientProvider";
import {ActionAppearance, CooldownNotification} from "../rpc/messages/ActionAppearance";
import {CooldownTracker} from "../render/CooldownTracker";
import {HotbarDisplay, ResolvedActionIcons} from "../render/HotbarDisplay";
import {GlobalSettingsProvider} from "../settings/GlobalSettingsProvider";

/**
 * A control that uses the hotbar action rendering system.
 */
export abstract class VirtualSlotControl<TSettings extends JsonObject> extends InteractiveControl<TSettings> {
    protected _appearance: ActionAppearance | undefined;

    private _iconId: number | undefined;
    private _iconBase64: string | undefined;

    protected readonly cooldownTracker: CooldownTracker;
    private readonly _hotbarDisplay: HotbarDisplay;

    protected constructor(
        ev: WillAppearEvent<TSettings>, clients: ClientProvider, settingsKind: string,
        globalSettings: GlobalSettingsProvider,
    ) {
        super(ev, clients, settingsKind);
        this.cooldownTracker = new CooldownTracker(() => void this._onCooldownTick(), globalSettings);
        this._hotbarDisplay = new HotbarDisplay(this.cooldownTracker);

        this.onClientEvent("Cooldown.CooldownStart", detail => this._onCooldownStart(detail));
    }

    cleanup(): void {
        this.cooldownTracker.stop();
        super.cleanup();
    }

    protected extraIcons(): Omit<ResolvedActionIcons, "iconBase64"> {
        return {};
    }

    protected async applyAppearance(appearance: ActionAppearance): Promise<void> {
        if (appearance.iconId !== this._iconId) {
            this._iconBase64 = await this.activeClient.icons.getBase64(appearance.iconId);
            this._iconId = appearance.iconId;
        }

        this._appearance = appearance;
        this.cooldownTracker.sync(appearance);
        await this._draw();
    }

    private async _draw(): Promise<void> {
        if (!this._appearance || !this._iconBase64) return;

        const svg = this._hotbarDisplay.render(this._appearance, {iconBase64: this._iconBase64, ...this.extraIcons()});
        await this.setImage(svg);
    }

    private async _onCooldownTick(): Promise<void> {
        if (this._appearance && this.cooldownTracker.isActive(this._appearance)) {
            await this._draw();
            return;
        }

        await this.safeRender();
    }

    private async _onCooldownStart(detail: CooldownNotification): Promise<void> {
        if (!this.cooldownTracker.isTracked(detail)) return;

        streamDeck.logger.debug("Received actionable cooldown notification", detail);

        await this.safeRender();
    }
}

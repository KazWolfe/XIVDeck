import {KeyDownEvent, WillAppearEvent} from "@elgato/streamdeck";
import {VirtualSlotControl} from "../VirtualSlotControl";
import {ClientProvider} from "../../rpc/ClientProvider";
import {HotbarButtonSettings} from "../../settings/types/HotbarButtonSettings";
import {GlobalSettingsProvider} from "../../settings/GlobalSettingsProvider";
import {HotbarWatchRegistry} from "../../rpc/HotbarWatchRegistry";

export class HotbarButton extends VirtualSlotControl<HotbarButtonSettings> {
    constructor(
        ev: WillAppearEvent<HotbarButtonSettings>, clients: ClientProvider, globalSettings: GlobalSettingsProvider,
        private readonly watches: HotbarWatchRegistry,
    ) {
        super(ev, clients, "hotbar", globalSettings);
    }

    async loadSettings(raw: unknown, migrate: boolean): Promise<void> {
        await super.loadSettings(raw, migrate);

        if (this.settings) {
            this.watches.watch(this.context, this.settings, () => void this.safeRender());
        } else {
            this.watches.release(this.context);
        }
    }

    cleanup(): void {
        this.watches.release(this.context);
        super.cleanup();
    }

    protected async render(): Promise<void> {
        const client = this.activeClient;
        const settings = this.settings;

        if (!client.isReady() || !settings) {
            return;
        }

        const appearance = await client.request("Hotbar.GetHotbarSlot", {
            hotbarId: settings.hotbarId,
            slotId: settings.slotId,
        });

        await this.applyAppearance(appearance);
    }

    async onKeyDown(_ev: KeyDownEvent<HotbarButtonSettings>): Promise<void> {
        if (!this.settings) {
            throw new Error("No hotbarId/slotId configured for this button.");
        }

        await this.activeClient.request("Hotbar.TriggerHotbarSlot", {
            hotbarId: this.settings.hotbarId,
            slotId: this.settings.slotId,
        });
    }
}

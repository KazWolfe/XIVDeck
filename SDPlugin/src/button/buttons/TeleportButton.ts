import { BaseButton } from "../BaseButton";
import { KeyDownEvent, WillAppearEvent } from "@rweich/streamdeck-events/dist/Events/Received/Plugin";
import plugin from "../../plugin";
import { FFXIVApi } from "../../link/ffxivplugin/FFXIVApi";
import { DidReceiveSettingsEvent } from "@rweich/streamdeck-events/dist/Events/Received";

export type TeleportButtonSettings = {
    aetheryteId: number;
    subId: number;

    cache: {
        name: string,
        region: string,
        territory: string
        isHousing: boolean
    }
}

export class TeleportButton extends BaseButton {
    useGameIcon: boolean = true;

    settings?: TeleportButtonSettings;

    constructor(event: WillAppearEvent) {
        super(event.context);

        this._xivEventListeners.add(plugin.xivPluginLink.on("_ready", this.render.bind(this)));

        this._sdEventListeners.set("keyDown", this.onKeyDown.bind(this));

        this.onReceivedSettings(event);
    }

    async onReceivedSettings(event: DidReceiveSettingsEvent | WillAppearEvent) {
        this.settings = event.settings as TeleportButtonSettings;
        this.render();
    }

    async onKeyDown(event: KeyDownEvent): Promise<void> {
        if (this.settings?.aetheryteId == undefined || this.settings?.subId) {
            throw new Error("An aetheryte was not specified for this button");
        }

        await FFXIVApi.Teleport.triggerTeleport(this.settings.aetheryteId, this.settings.subId);
    }

    async render(): Promise<void> {
        // no-op, no dynamic icon loading (yet)
    }
}
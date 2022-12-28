import {BaseButton} from "../BaseButton";
import {KeyDownEvent, WillAppearEvent} from "@rweich/streamdeck-events/dist/Events/Received/Plugin";
import plugin from "../../plugin";
import {FFXIVApi} from "../../link/ffxivplugin/FFXIVApi";
import {StateMessage} from "../../link/ffxivplugin/GameTypes";
import {DidReceiveSettingsEvent} from "@rweich/streamdeck-events/dist/Events/Received";

export type HotbarButtonSettings = {
    hotbarId: number,
    slotId: number
}

export class HotbarButton extends BaseButton {

    settings?: HotbarButtonSettings;
    
    constructor(event: WillAppearEvent) {
        super(event.context);
        
        this._xivEventListeners.add(plugin.xivPluginLink.on("_ready", this.render.bind(this)));
        this._xivEventListeners.add(plugin.xivPluginLink.on("stateUpdate", this.stateUpdate.bind(this)));
        
        this._sdEventListeners.set("keyDown", this.onKeyDown.bind(this));
        
        this.onReceivedSettings(event);
    }
    
    async onReceivedSettings(event: DidReceiveSettingsEvent | WillAppearEvent) {
        this.settings = event.settings as HotbarButtonSettings;
        this.render();
    }

    async onKeyDown(event: KeyDownEvent): Promise<void> {
        if (this.settings?.hotbarId == undefined || this.settings?.slotId == undefined) {
            throw new Error("No hotbarId/slotId defined for this button");
        }
        
        await FFXIVApi.Hotbar.triggerHotbarSlot(this.settings.hotbarId, this.settings.slotId);
    }
    
    async stateUpdate(message: StateMessage) : Promise<void> {
        if (message.type != "Hotbar") return;
        
        await this.render();
    }
    
    async render() : Promise<void> {
        if (!plugin.xivPluginLink.isReady()) {
            return;
        }

        if (this.settings?.hotbarId == undefined || this.settings?.slotId == undefined) {
            return
        }
        
        let response = await FFXIVApi.Hotbar.getHotbarSlot(this.settings.hotbarId, this.settings.slotId);
        this.setImage(response.iconData);
    }
}
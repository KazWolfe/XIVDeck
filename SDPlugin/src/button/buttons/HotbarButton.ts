import {BaseButton} from "../BaseButton";
import AbstractStateEvent from "@rweich/streamdeck-events/dist/Events/Received/Plugin/AbstractStateEvent";
import {KeyDownEvent} from "@rweich/streamdeck-events/dist/Events/Received/Plugin";
import plugin from "../../plugin";
import {FFXIVApi} from "../../link/ffxivplugin/FFXIVApi";

export type HotbarButtonSettings = {
    hotbarId: number,
    slotId: number
}

export class HotbarButton extends BaseButton {

    // Information about the action that this button will be triggering
    hotbarId: number;
    slotId: number;
    
    constructor(event: AbstractStateEvent) {
        super(event.context);
        
        let settings = event.settings as HotbarButtonSettings
        
        this.hotbarId = settings.hotbarId;
        this.slotId = settings.slotId;
        
        // render if already exists and set up hooks to wait for events
        this._xivEventListeners.add(plugin.xivPluginLink.on("initReply", this.render.bind(this)));
        this._xivEventListeners.add(plugin.xivPluginLink.on("hotbarUpdate", this.handleUpdate.bind(this)));
        this.render();
    }

    async execute(event: KeyDownEvent): Promise<void> {
        if (this.hotbarId == null || this.slotId == null) {
            throw new Error("No hotbarId/slotId defined for this button");
        }
        
        await FFXIVApi.Hotbar.triggerHotbarSlot(this.hotbarId, this.slotId);
    }
    
    async handleUpdate() : Promise<void> {
        await this.render();
    }
    
    async render() : Promise<void> {
        if (!plugin.xivPluginLink.isReady()) {
            return;
        }

        if (this.hotbarId == null || this.slotId == null) {
            return
        }
        
        let response = await FFXIVApi.Hotbar.getHotbarSlot(this.hotbarId, this.slotId);
        this.setImage(response.iconData);
    }
}
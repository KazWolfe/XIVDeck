import {BaseButton} from "../BaseButton";
import AbstractStateEvent from "@rweich/streamdeck-events/dist/Events/Received/Plugin/AbstractStateEvent";
import {KeyDownEvent} from "@rweich/streamdeck-events/dist/Events/Received/Plugin";
import {
    ExecuteHotbarSlotOpcode,
    GetHotbarSlotIconOpcode,
    HotbarSlotIconMessage
} from "../../link/ffxivplugin/messages/HotbarMessages";
import plugin from "../../plugin";

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

    execute(event: KeyDownEvent): void {
        if (this.hotbarId == null || this.slotId == null) {
            throw new Error("No hotbarId/slotId defined for this button");
        }
        
        plugin.xivPluginLink.send(new ExecuteHotbarSlotOpcode(this.hotbarId, this.slotId));
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
        
        let response: HotbarSlotIconMessage;
        response = await plugin.xivPluginLink.send(new GetHotbarSlotIconOpcode(this.hotbarId, this.slotId)) as HotbarSlotIconMessage;
        this.setImage(response.iconData);
    }
}
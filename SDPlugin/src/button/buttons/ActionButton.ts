import {BaseButton} from "../BaseButton"
import {KeyDownEvent, WillAppearEvent} from "@rweich/streamdeck-events/dist/Events/Received/Plugin";
import plugin from "../../plugin";
import {StateMessage} from "../../link/ffxivplugin/GameTypes";
import {FFXIVApi} from "../../link/ffxivplugin/FFXIVApi";
import {DidReceiveSettingsEvent} from "@rweich/streamdeck-events/dist/Events/Received";

export type ActionButtonSettings = {
    actionType: string,
    actionId: number,
    actionName: string
}

export class ActionButton extends BaseButton {
    settings?: ActionButtonSettings;
    
    useGameIcon: boolean = true;
    
    constructor(event: WillAppearEvent) {
        super(event.context);
        
        this._xivEventListeners.add(plugin.xivPluginLink.on("_ready", this.render.bind(this)));
        this._xivEventListeners.add(plugin.xivPluginLink.on("stateUpdate", this.stateUpdate.bind(this)));
        
        this.onReceivedSettings(event);
    }
    
    async onReceivedSettings(event: DidReceiveSettingsEvent | WillAppearEvent): Promise<void> {
        this.settings = event.settings as ActionButtonSettings;
        this.render();
    }

    async execute(event: KeyDownEvent): Promise<void> {
        if (this.settings?.actionType == undefined || this.settings?.actionId == undefined) {
            throw Error("Not action type/ID was defined for this button!");
        }
        
        await FFXIVApi.Action.executeAction(this.settings.actionType, this.settings.actionId);
    }
    
    async render() {
        if (!this.useGameIcon) {
            this.setImage("");
            return
        }
        
        if (!plugin.xivPluginLink.isReady()) {
            return;
        }

        if (this.settings?.actionType == undefined || this.settings?.actionId == undefined) {
            return;
        }
        
        let actionInfo = await FFXIVApi.Action.getAction(this.settings.actionType, this.settings.actionId);
        this.setImage(await FFXIVApi.getIcon(actionInfo.iconId));
    }
    
    private stateUpdate(message: StateMessage) {
        if (this.settings?.actionType == "GearSet" && message.type == "GearSet") {
            this.render();
        }
    }
}
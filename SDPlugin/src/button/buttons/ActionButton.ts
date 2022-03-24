import {BaseButton} from "../BaseButton"
import {KeyDownEvent} from "@rweich/streamdeck-events/dist/Events/Received/Plugin";
import AbstractStateEvent from "@rweich/streamdeck-events/dist/Events/Received/Plugin/AbstractStateEvent";
import plugin from "../../plugin";
import {StateMessage} from "../../link/ffxivplugin/GameTypes";
import {FFXIVApi} from "../../link/ffxivplugin/FFXIVApi";

export type ActionButtonSettings = {
    actionType: string,
    actionId: number
}

export class ActionButton extends BaseButton {
    actionType: string;
    actionId: number;
    
    useGameIcon: boolean = true;
    
    constructor(event: AbstractStateEvent) {
        super(event.context);

        let settings = event.settings as ActionButtonSettings;

        this.actionType = settings.actionType;
        this.actionId = settings.actionId;

        this._xivEventListeners.add(plugin.xivPluginLink.on("initReply", this.render.bind(this)));
        this._xivEventListeners.add(plugin.xivPluginLink.on("stateUpdate", this.stateUpdate.bind(this)));
        this.render();
    }

    async execute(event: KeyDownEvent): Promise<void> {
        if (this.actionType == null || this.actionId == null) {
            throw Error("Not action type/ID was defined for this button!");
        }
        
        await FFXIVApi.Action.executeAction(this.actionType, this.actionId);
    }
    
    async render() {
        if (!this.useGameIcon) {
            this.setImage("");
            return
        }
        
        if (!plugin.xivPluginLink.isReady()) {
            return;
        }

        if (this.actionType == null || this.actionId == null) {
            return;
        }
        
        let actionInfo = await FFXIVApi.Action.getAction(this.actionType, this.actionId);
        this.setImage(await FFXIVApi.getIcon(actionInfo.iconId));
    }
    
    private stateUpdate(message: StateMessage) {
        if (this.actionType == "GearSet" && message.type == "GearSet") {
            this.render();
        }
    }
}
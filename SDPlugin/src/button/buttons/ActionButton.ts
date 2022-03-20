import {BaseButton} from "../BaseButton"
import {KeyDownEvent} from "@rweich/streamdeck-events/dist/Events/Received/Plugin";
import AbstractStateEvent from "@rweich/streamdeck-events/dist/Events/Received/Plugin/AbstractStateEvent";
import plugin from "../../plugin";
import {
    ActionIconMessage,
    ExecuteActionOpcode,
    GetActionIconOpcode
} from "../../link/ffxivplugin/messages/ActionMessages";
import {GetHotbarSlotIconOpcode, HotbarSlotIconMessage} from "../../link/ffxivplugin/messages/HotbarMessages";
import {StateMessage} from "../../link/ffxivplugin/GameTypes";

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

    execute(event: KeyDownEvent): void {
        if (this.actionType == null || this.actionId == null) {
            throw Error("Not action type/ID was defined for this button!");
        }
        
        plugin.xivPluginLink.send(new ExecuteActionOpcode(this.actionType, this.actionId));
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

        let response: ActionIconMessage;
        response = await plugin.xivPluginLink.send(new GetActionIconOpcode(this.actionType, this.actionId)) as ActionIconMessage;

        this.setImage(response.iconData);
    }
    
    private stateUpdate(message: StateMessage) {
        if (this.actionType == "GearSet" && message.type == "gearSet") {
            this.render();
        }
    }
}
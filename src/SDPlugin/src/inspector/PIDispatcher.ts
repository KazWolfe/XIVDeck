import {BaseFrame} from "./BaseFrame";
import piInstance from "../inspector";
import {DidReceiveSettingsEvent} from "@rweich/streamdeck-events/dist/Events/Received";
import {CommandFrame} from "./frames/CommandFrame";
import {HotbarFrame} from "./frames/HotbarFrame";
import {ActionFrame} from "./frames/ActionFrame";
import {MacroFrame} from "./frames/MacroFrame";
import {ClassFrame} from "./frames/ClassFrame";
import {VolumeFrame} from "./frames/VolumeFrame";

export class PIDispatcher {
    public piFrame?: BaseFrame<unknown>;
    
    constructor() { }
    
    initialize() {
        let actionType = piInstance.sdPluginLink.actionInfo?.action;

        switch (actionType) {
            case "dev.wolf.xivdeck.sdplugin.actions.sendcommand":
                this.piFrame = new CommandFrame();
                break;
            case "dev.wolf.xivdeck.sdplugin.actions.exechotbar":
                this.piFrame = new HotbarFrame();
                break;
            case "dev.wolf.xivdeck.sdplugin.actions.execaction":
                this.piFrame = new ActionFrame();
                break;
            case "dev.wolf.xivdeck.sdplugin.actions.execmacro":
                this.piFrame = new MacroFrame();
                break;
            case "dev.wolf.xivdeck.sdplugin.actions.switchclass":
                this.piFrame = new ClassFrame();
                break;
            case "dev.wolf.xivdeck.sdplugin.actions.volume":
                this.piFrame = new VolumeFrame();
                break;
            default:
                throw new Error(`Undefined action type: ${actionType}`)
        }

        this.piFrame.renderHTML();
        this.piFrame.loadSettings(piInstance.sdPluginLink.actionInfo?.settings);
    }
    
    handleReceivedSettings(event: DidReceiveSettingsEvent) {
        if (this.piFrame == null) return;
        
        this.piFrame.loadSettings(event.settings)
    }
}
import {BaseButton} from "../BaseButton";
import AbstractStateEvent from "@rweich/streamdeck-events/dist/Events/Received/Plugin/AbstractStateEvent";
import {KeyDownEvent, WillAppearEvent} from "@rweich/streamdeck-events/dist/Events/Received/Plugin";
import plugin from "../../plugin";
import {FFXIVApi} from "../../link/ffxivplugin/FFXIVApi";
import {StateMessage} from "../../link/ffxivplugin/GameTypes";
import {DidReceiveSettingsEvent} from "@rweich/streamdeck-events/dist/Events/Received";

export type MacroButtonSettings = { 
    macroId: number
}

export class MacroButton extends BaseButton {
    
    settings?: MacroButtonSettings;
    useGameIcon: boolean = true;

    constructor(event: WillAppearEvent) {
        super(event.context);

        this._xivEventListeners.add(plugin.xivPluginLink.on("_ready", this.render.bind(this)));
        this._xivEventListeners.add(plugin.xivPluginLink.on("stateUpdate", this.stateUpdate.bind(this)));
        
        this.onReceivedSettings(event);
    }
    
    async onReceivedSettings(event: DidReceiveSettingsEvent | WillAppearEvent) {
        this.settings = event.settings as MacroButtonSettings;
        await this.render();
    }

    async execute(event: KeyDownEvent): Promise<void> {
        if (this.settings?.macroId == undefined) {
            throw Error("No macro ID was defined for this button!");
        }
        
        await FFXIVApi.Action.executeAction("Macro", this.settings.macroId);
    }

    async stateUpdate(message: StateMessage) : Promise<void> {
        if (message.type != "Macro") return;

        await this.render();
    }

    async render() {
        if (!this.useGameIcon) {
            this.setImage("");
            return
        }

        if (!plugin.xivPluginLink.isReady() || this.settings?.macroId == undefined) {
            return;
        }
        
        let actionData = await FFXIVApi.Action.getAction("Macro", this.settings.macroId);
        this.setImage(await FFXIVApi.getIcon(actionData.iconId));
    }
}
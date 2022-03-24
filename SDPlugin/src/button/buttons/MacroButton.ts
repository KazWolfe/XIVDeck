import {BaseButton} from "../BaseButton";
import AbstractStateEvent from "@rweich/streamdeck-events/dist/Events/Received/Plugin/AbstractStateEvent";
import {KeyDownEvent} from "@rweich/streamdeck-events/dist/Events/Received/Plugin";
import plugin from "../../plugin";
import {FFXIVApi} from "../../link/ffxivplugin/FFXIVApi";

export type MacroButtonSettings = { 
    macroId: number
}

export class MacroButton extends BaseButton {
    // The macro this button will trigger
    macroId: number;
    
    useGameIcon: boolean = true;

    constructor(event: AbstractStateEvent) {
        super(event.context);

        let settings = event.settings as MacroButtonSettings;
        this.macroId = settings.macroId;

        this.render();
        this._xivEventListeners.add(plugin.xivPluginLink.on("initReply", this.render.bind(this)));
    }

    async execute(event: KeyDownEvent): Promise<void> {
        if (this.macroId == null) {
            throw Error("No macro ID was defined for this button!");
        }
        
        await FFXIVApi.Action.executeAction("Macro", this.macroId);
    }

    async render() {
        if (!this.useGameIcon) {
            this.setImage("");
            return
        }

        if (!plugin.xivPluginLink.isReady()) {
            return;
        }

        if (this.macroId == null) {
            return
        }
        
        let actionData = await FFXIVApi.Action.getAction("Macro", this.macroId);
        this.setImage(await FFXIVApi.getIcon(actionData.iconId));
    }
}
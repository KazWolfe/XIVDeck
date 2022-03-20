import {BaseButton} from "../BaseButton";
import AbstractStateEvent from "@rweich/streamdeck-events/dist/Events/Received/Plugin/AbstractStateEvent";
import {KeyDownEvent} from "@rweich/streamdeck-events/dist/Events/Received/Plugin";
import plugin from "../../plugin";
import {
    ActionIconMessage,
    ExecuteActionOpcode,
    GetActionIconOpcode
} from "../../link/ffxivplugin/messages/ActionMessages";

type MacroButtonSettings = { 
    isSharedMacro: boolean,
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

    execute(event: KeyDownEvent): void {
        if (this.macroId == null) {
            throw Error("No macro ID was defined for this button!");
        }

        plugin.xivPluginLink.send(new ExecuteActionOpcode("Macro", this.macroId));
    }

    async render() {
        if (!plugin.xivPluginLink.isReady()) {
            return;
        }

        if (!this.useGameIcon) {
            this.setImage("");
            return
        }

        let response: ActionIconMessage;
        response = await plugin.xivPluginLink.send(new GetActionIconOpcode("Macro", this.macroId)) as ActionIconMessage;

        this.setImage(response.iconData);
    }
}
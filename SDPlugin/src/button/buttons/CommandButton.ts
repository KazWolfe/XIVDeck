import {BaseButton} from "../BaseButton";
import AbstractStateEvent from "@rweich/streamdeck-events/dist/Events/Received/Plugin/AbstractStateEvent";
import {KeyDownEvent} from "@rweich/streamdeck-events/dist/Events/Received/Plugin";
import plugin from "../../plugin";
import {ExecuteCommandOpcode} from "../../link/ffxivplugin/messages/CommandMessages";

type CommandButtonSettings = {
    command: string
}

export class CommandButton extends BaseButton {
    command: string;
    
    constructor(event: AbstractStateEvent) {
        super(event.context);
        
        let settings = event.settings as CommandButtonSettings;
        this.command = settings.command;
    }

    execute(event: KeyDownEvent): void {
        if (this.command == null) {
            throw new Error("No command specified for this button");
        }
        
        plugin.xivPluginLink.send(new ExecuteCommandOpcode(this.command))
    }
}
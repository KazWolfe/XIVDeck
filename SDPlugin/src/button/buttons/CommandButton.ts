import {BaseButton} from "../BaseButton";
import AbstractStateEvent from "@rweich/streamdeck-events/dist/Events/Received/Plugin/AbstractStateEvent";
import {KeyDownEvent} from "@rweich/streamdeck-events/dist/Events/Received/Plugin";
import plugin from "../../plugin";
import {ExecuteCommandOpcode} from "../../link/ffxivplugin/messages/CommandMessages";

export type CommandButtonSettings = {
    command: string
}

export class CommandButton extends BaseButton {
    command: string;
    
    constructor(event: AbstractStateEvent) {
        super(event.context);
        
        let settings = event.settings as CommandButtonSettings;
        this.command = settings.command;
    }

    async execute(event: KeyDownEvent): Promise<void> {
        if (this.command == null || this.command === "/") {
            throw new Error("No command specified for this button");
        }
        
       await this._sendExec(new ExecuteCommandOpcode(this.command));
    }
}
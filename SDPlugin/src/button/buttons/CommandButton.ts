import {BaseButton} from "../BaseButton";
import AbstractStateEvent from "@rweich/streamdeck-events/dist/Events/Received/Plugin/AbstractStateEvent";
import {KeyDownEvent, WillAppearEvent} from "@rweich/streamdeck-events/dist/Events/Received/Plugin";
import {FFXIVApi} from "../../link/ffxivplugin/FFXIVApi";
import {DidReceiveSettingsEvent} from "@rweich/streamdeck-events/dist/Events/Received";

export type CommandButtonSettings = {
    command: string
}

export class CommandButton extends BaseButton {
    settings?: CommandButtonSettings;
    
    constructor(event: WillAppearEvent) {
        super(event.context);
        
        this.onReceivedSettings(event);
    }
    
    async onReceivedSettings(event: DidReceiveSettingsEvent | WillAppearEvent) {
        this.settings = event.settings as CommandButtonSettings;
    }

    async execute(event: KeyDownEvent): Promise<void> {
        if (this.settings?.command == null || this.settings.command === "/") {
            throw new Error("No command specified for this button");
        }
        
       await FFXIVApi.runTextCommand(this.settings.command);
    }
    
    async render() {
        // nothing to render for CommandButtons
        return;
    }
}
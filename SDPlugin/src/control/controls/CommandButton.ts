import {KeyDownEvent, WillAppearEvent} from "@elgato/streamdeck";
import {InteractiveControl} from "../InteractiveControl";
import {ClientProvider} from "../../rpc/ClientProvider";
import {CommandButtonSettings} from "../../settings/types/CommandButtonSettings";

export class CommandButton extends InteractiveControl<CommandButtonSettings> {
    constructor(ev: WillAppearEvent<CommandButtonSettings>, clients: ClientProvider) {
        super(ev, clients, "command");
    }

    protected async render(): Promise<void> {
        // nothing to render
    }

    async onKeyDown(_ev: KeyDownEvent<CommandButtonSettings>): Promise<void> {
        if (!this.settings) {
            throw new Error("No command specified for this button.");
        }

        await this.activeClient.request("Command.ExecuteCommand", {
            commandRequest: {
                command: this.settings.command,
            },
        });
    }
}

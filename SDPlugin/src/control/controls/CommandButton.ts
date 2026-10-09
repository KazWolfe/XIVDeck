import {injectable} from "inversify";
import {KeyDownEvent} from "@elgato/streamdeck";
import {JsonObject} from "@elgato/utils";
import {Control} from "#/control/Control";
import {IKeyControl} from "#/control/ControlInput";
import {ControlContext} from "#/control/ControlContext";
import {ClientProxy} from "#/control/proxy/ClientProxy";
import {CommandButtonSettings} from "#/settings/types/CommandButtonSettings";

@injectable()
export class CommandButton extends Control<CommandButtonSettings> implements IKeyControl {
    public constructor(context: ControlContext, client: ClientProxy) {
        super(context, client);
    }

    public async onKeyDown(_ev: KeyDownEvent<JsonObject>): Promise<void> {
        await this.client.request("Command.ExecuteCommand", {
            commandRequest: {
                command: this.tryReadSeconds().command,
            },
        });
    }

    protected async render(): Promise<void> {
        // nothing to render
    }
}

import {injectable} from "inversify";
import {KeyAction, KeyDownEvent} from "@elgato/streamdeck";
import {JsonObject} from "@elgato/utils";
import {Control} from "#/control/Control";
import {IKeyControl} from "#/control/ControlInput";
import {ControlContext} from "#/control/ControlContext";
import {ClientProxy} from "#/control/proxy/ClientProxy";
import {ClassButtonSettings} from "#/settings/types/ClassButtonSettings";

@injectable()
export class ClassButton extends Control<ClassButtonSettings> implements IKeyControl {
    private readonly _action: KeyAction<ClassButtonSettings>;
    private _lastImage: string | null | undefined;

    public constructor(context: ControlContext, client: ClientProxy) {
        super(context, client);
        this._action = context.asKey<ClassButtonSettings>();
    }

    public async onKeyDown(_ev: KeyDownEvent<JsonObject>): Promise<void> {
        await this.client.request("ClassJob.SwitchClass", {id: this.tryReadSeconds().classId});
    }

    public override async handlePiRequest(command: string, params: unknown): Promise<unknown> {
        if (command !== "listClasses") {
            return super.handlePiRequest(command, params);
        }

        return (await this.client.request("ClassJob.GetAvailableClasses")).classes;
    }

    protected async render(): Promise<void> {
        const settings = this.settings;
        if (!settings) {
            return;
        }

        if (!this.client.isAvailable) {
            await this.present(null);
            return;
        }

        const classInfo = await this.client.request("ClassJob.GetClass", {id: settings.classId});

        await this.saveSettings({...settings, cache: classInfo});

        await this.present(`data:image/png;base64,${await this.client.getIcon(classInfo.iconId)}`);
    }

    /** `null` resets to the default image. */
    private async present(image: string | null): Promise<void> {
        if (image === this._lastImage) {
            return;
        }

        await this._action.setImage(image ?? undefined);
        this._lastImage = image;
    }
}

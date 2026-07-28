import {KeyDownEvent, WillAppearEvent} from "@elgato/streamdeck";
import {InteractiveControl} from "../InteractiveControl";
import {ClientProvider} from "../../rpc/ClientProvider";
import {ClassButtonSettings} from "../../settings/types/ClassButtonSettings";

export class ClassButton extends InteractiveControl<ClassButtonSettings> {
    constructor(ev: WillAppearEvent<ClassButtonSettings>, clients: ClientProvider) {
        super(ev, clients, "class");
    }

    protected async render(): Promise<void> {
        const client = this.activeClient;
        const settings = this.settings;

        if (!client.isReady() || !settings) {
            return;
        }

        const classInfo = await client.request("ClassJob.GetClass", {id: settings.classId});

        if (JSON.stringify(classInfo) !== JSON.stringify(settings.cache)) {
            this.settings = {...settings, cache: classInfo};
            await this.action.setSettings(this.settings);
        }

        const iconBase64 = await client.icons.getBase64(classInfo.iconId);
        await this.setImage(`data:image/png;base64,${iconBase64}`);
    }

    async onKeyDown(_ev: KeyDownEvent<ClassButtonSettings>): Promise<void> {
        if (!this.settings) {
            throw new Error("No class configured for this button.");
        }

        await this.activeClient.request("ClassJob.SwitchClass", {id: this.settings.classId});
    }

    async handlePiRequest(command: string, params: unknown): Promise<unknown> {
        if (command !== "listClasses") {
            return super.handlePiRequest(command, params);
        }

        return (await this.activeClient.request("ClassJob.GetAvailableClasses")).classes;
    }
}

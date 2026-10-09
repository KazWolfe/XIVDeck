import {Control} from "#/control/Control";
import {ControlContext} from "#/control/ControlContext";
import {ClientProxy} from "#/control/proxy/ClientProxy";
import {GameNotificationProxy} from "#/control/proxy/GameNotificationProxy";
import {VolumeControlSettings} from "#/settings/types/VolumeControlSettings";
import {VolumeState} from "#/client/rpc/messages/Volume";

/**
 * Base class for the two volume controllers.
 */
export abstract class VolumeControlBase extends Control<VolumeControlSettings> {
    protected constructor(context: ControlContext, client: ClientProxy, notifications: GameNotificationProxy) {
        super(context, client);

        notifications.on("Volume.VolumeChanged", this.onVolumeChanged.bind(this));
    }

    public override async handlePiRequest(command: string, params: unknown): Promise<unknown> {
        if (command !== "listChannels") {
            return super.handlePiRequest(command, params);
        }

        // Read the names off the values: they're serialized by name, whatever the dictionary keys come out as.
        const {channels} = await this.client.request("Volume.GetAllChannels");
        return Object.values(channels).map(state => state.channel);
    }

    protected async render(): Promise<void> {
        const settings = this.settings;
        if (!this.client.isAvailable || !settings) {
            await this.renderNoData();
            return;
        }

        await this.applyState(settings, await this.client.request("Volume.GetChannel", {channel: settings.channel}));
    }

    protected abstract renderNoData(): Promise<void>;

    protected abstract applyState(settings: VolumeControlSettings, state: VolumeState): Promise<void>;

    private async onVolumeChanged(state: VolumeState): Promise<void> {
        const settings = this.settings;
        if (settings?.channel !== state.channel) {
            return;
        }

        try {
            await this.applyState(settings, state);
        } catch (err) {
            this.context.logError("rendering pushed volume state for", err);
        }
    }
}

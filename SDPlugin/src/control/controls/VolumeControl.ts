import streamDeck, {DialDownEvent, DialRotateEvent, KeyDownEvent, TouchTapEvent, WillAppearEvent} from "@elgato/streamdeck";
import {InteractiveControl} from "../InteractiveControl";
import {ClientProvider} from "../../rpc/ClientProvider";
import {VolumeControlMode, VolumeControlSettings} from "../../settings/types/VolumeControlSettings";
import {VolumeState} from "../../rpc/messages/Volume";
import {XivDeckClient} from "../../rpc/XivDeckClient";
import i18n from "../../i18n/i18n";

export class VolumeControl extends InteractiveControl<VolumeControlSettings> {
    constructor(ev: WillAppearEvent<VolumeControlSettings>, clients: ClientProvider) {
        super(ev, clients, "volume");

        this.onClientEvent("Volume.VolumeChanged", state => this._onVolumeChanged(state));
    }

    protected async render(): Promise<void> {
        await this._applyState(await this._fetchState());
    }

    private async _applyState(state: VolumeState | undefined): Promise<void> {
        if (this.action.isKey()) {
            await this._renderKey(state);
        } else if (this.action.isDial()) {
            await this._renderDial(state);
        }
    }

    private async _renderKey(state: VolumeState | undefined): Promise<void> {
        if (!this.action.isKey()) return;

        const settings = this.settings;
        if (!state || !settings) {
            await this.action.setState(0);
            return;
        }

        switch (settings.mode) {
            case VolumeControlMode.ADJUST:
                await this.action.setTitle(`${settings.multiplier >= 0 ? "+" : ""}${settings.multiplier}`);
                break;
            case VolumeControlMode.SET:
                await this.action.setTitle(`${settings.value}`);
                break;
            default:
                await this.action.setTitle(i18n.t(`soundChannels:${settings.channel}.short`));
                await this.action.setState(state.muted ? 1 : 0);
        }
    }

    private async _renderDial(state: VolumeState | undefined): Promise<void> {
        if (!this.action.isDial()) return;

        if (!state || !this.settings) {
            await this.action.setFeedback({
                value: "--",
                icon: "images/common/o_nodata.png",
                indicator: {value: 0, opacity: 0.6},
            });
            return;
        }

        await this.action.setFeedback({
            title: i18n.t(`soundChannels:${this.settings.channel}.full`),
            value: state.muted ? i18n.t("controls:VolumeControl.muted") : state.volume,
            icon: `images/states/volume/o_${state.muted ? "muted" : "unmuted"}.png`,
            indicator: {value: state.volume, opacity: 1.0, bar_fill_c: state.muted ? "red" : "white"},
        });
    }

    async onKeyDown(_ev: KeyDownEvent<VolumeControlSettings>): Promise<void> {
        this._requireClient();
        const settings = this._requireSettings();

        switch (settings.mode) {
            case VolumeControlMode.SET:
                await this._applyState(await this._requireClient().request("Volume.SetChannel", {
                    channel: settings.channel, volume: settings.value,
                }));
                break;

            case VolumeControlMode.ADJUST:
                await this._deltaChannel(settings.multiplier);
                break;

            default:
                await this._toggleMute();
        }
    }

    async onDialDown(_ev: DialDownEvent<VolumeControlSettings>): Promise<void> {
        await this._toggleMute();
    }

    async onTouchTap(_ev: TouchTapEvent<VolumeControlSettings>): Promise<void> {
        await this._toggleMute();
    }

    async onDialRotate(ev: DialRotateEvent<VolumeControlSettings>): Promise<void> {
        const settings = this._requireSettings();

        const step = settings.mode === VolumeControlMode.SET ? 1 : settings.multiplier ?? 1;
        await this._deltaChannel(ev.payload.ticks * step);
    }

    async handlePiRequest(command: string, params: unknown): Promise<unknown> {
        if (command !== "listChannels") {
            return super.handlePiRequest(command, params);
        }

        // Read the names off the values: they're serialized by name, whatever the dictionary keys come out as.
        const {channels} = await this.activeClient.request("Volume.GetAllChannels");
        return Object.values(channels).map(state => state.channel);
    }

    private async _deltaChannel(delta: number): Promise<void> {
        await this._applyState(await this._requireClient().request("Volume.DeltaChannel", {
            channel: this._requireSettings().channel, delta,
        }));
    }

    private async _toggleMute(): Promise<void> {
        await this._applyState(await this._requireClient().request("Volume.ToggleMuteChannel", {
            channel: this._requireSettings().channel,
        }));
    }

    private async _fetchState(): Promise<VolumeState | undefined> {
        const client = this.activeClient;
        if (!client.isReady() || !this.settings) return undefined;

        return client.request("Volume.GetChannel", {channel: this.settings.channel});
    }

    private _requireClient(): XivDeckClient {
        const client = this.activeClient;
        if (!client.isReady()) throw new Error("Game not running.");

        return client;
    }

    private _requireSettings(): VolumeControlSettings {
        if (!this.settings) throw new Error("Volume control is not configured.");

        return this.settings;
    }

    private async _onVolumeChanged(state: VolumeState): Promise<void> {
        if (this.settings?.channel !== state.channel) return;

        try {
            await this._applyState(state);
        } catch (err) {
            streamDeck.logger.warn("Failed to render pushed volume state:", err);
        }
    }
}

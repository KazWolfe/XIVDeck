import { DidReceiveSettingsEvent } from "@rweich/streamdeck-events/dist/Events/Received";
import {
    DialDownEvent,
    KeyDownEvent,
    TouchTapEvent,
    WillAppearEvent
} from "@rweich/streamdeck-events/dist/Events/Received/Plugin";
import { DialPressEvent, DialRotateEvent } from "@rweich/streamdeck-events/dist/Events/Received/Plugin/Dial";
import i18n from "../../i18n/i18n";
import { GameNotRunningError } from "../../link/ffxivplugin/exceptions/Exceptions";
import { FFXIVApi } from "../../link/ffxivplugin/FFXIVApi";
import { FFXIVPluginLink } from "../../link/ffxivplugin/FFXIVPluginLink";
import { VolumeMessage, VolumePayload } from "../../link/ffxivplugin/GameTypes";
import { SetVolume } from "../../link/ffxivplugin/messages/outbound/SetVolume";
import plugin from "../../plugin";
import { BaseButton } from "../BaseButton";
import { VolumeButtonMode, VolumeButtonSettings } from "../settings/VolumeButtonSettings";

export class VolumeButton extends BaseButton {
    useGameIcon: boolean = true;
    isDial: boolean = false;

    settings?: VolumeButtonSettings;
    lastState?: VolumePayload;

    constructor(event: WillAppearEvent) {
        super(event.context);

        this.isDial = (event.controller == "Encoder");
        
        this._xivEventListeners.add(plugin.xivPluginLink.on("_ready", this.loadFromGame.bind(this)));
        this._xivEventListeners.add(plugin.xivPluginLink.on("volumeUpdate", this.onVolumeUpdate.bind(this)));
        this._xivEventListeners.add(plugin.xivPluginLink.on("_wsClosed", this.renderInvalidState.bind(this)));

        this._sdEventListeners.set("touchTap", this.onPress.bind(this));
        this._sdEventListeners.set("keyDown", this.onKeyDown.bind(this));
        this._sdEventListeners.set("dialPress", this.onPress.bind(this));
        this._sdEventListeners.set("dialDown", this.onPress.bind(this));
        this._sdEventListeners.set("dialRotate", this.onDialRotate.bind(this));

        this.onReceivedSettings(event);
    }

    async onReceivedSettings(event: DidReceiveSettingsEvent | WillAppearEvent) {
        this.settings = event.settings as VolumeButtonSettings;
        await this.loadFromGame();
    }

    async onDialRotate(event: DialRotateEvent): Promise<void> {
        this.preEventGuard();

        await FFXIVPluginLink.instance.send(new SetVolume(this.settings!.channel, {
            delta: event.ticks * (this.settings?.multiplier ?? 1)
        }));
    }

    async onPress(event: DialPressEvent | DialDownEvent | TouchTapEvent): Promise<void> {
        // ignore dial release events specifically
        if (event instanceof DialPressEvent && !event.pressed) return;

        this.preEventGuard();

        await FFXIVPluginLink.instance.send(new SetVolume(this.settings!.channel, {
            muted: !this.lastState!.muted
        }));
    }

    async onKeyDown(event: KeyDownEvent) {
        this.preEventGuard();
        
        const buttonMode = this.settings?.mode ?? VolumeButtonMode.MUTE;
        let payload = undefined;

        switch (buttonMode) {
            case VolumeButtonMode.MUTE:
                payload = {muted: !this.lastState!.muted};
                break;

            case VolumeButtonMode.SET:
                if (this.settings?.value == null)
                    throw Error("Value not set on volume button!");

                payload = {volume: this.settings.value};
                break;

            case VolumeButtonMode.ADJUST:
                if (this.settings?.multiplier == null)
                    throw Error("Multiplier not set on volume button!");

                payload = {delta: this.settings.multiplier};
                break;
        }

        await FFXIVPluginLink.instance.send(new SetVolume(this.settings!.channel, payload));
    }

    async onVolumeUpdate(message: VolumeMessage): Promise<void> {
        if (message.channel != this.settings?.channel) return;

        this.lastState = message.data;

        await this.render();
    }

    async render() {
        if (this.settings?.channel == undefined || !this.lastState) {
            this.renderInvalidState();
            return;
        }

        const muted = this.lastState?.muted;
        
        if (this.isDial) {
            this.setFeedback({
                title: this.getChannelName(),
                value: muted ? "Muted" : this.lastState.volume,
                icon: `images/states/volume/o_${muted ? 'muted' : 'unmuted'}.png`,
                indicator: {
                    value: this.lastState.volume,
                    opacity: 1.0,
                    bar_fill_c: muted ? "red" : null
                }
            });
            return;
        }
        switch (this.settings?.mode) {
            case VolumeButtonMode.ADJUST:
                this.setTitle(`${this.getChannelShort()}\n${(this.settings.multiplier ?? 0) >= 0 ? "+" : ""}${this.settings.multiplier}`);
                break;
            case VolumeButtonMode.SET:
                this.setTitle(`${this.getChannelShort()}\n${this.settings.value}`);
                break;
            default:
            case VolumeButtonMode.MUTE:
                this.setTitle(this.getChannelShort());
                this.setState(muted ? 1 : 0);
        }
    }

    private async loadFromGame() {
        if (!plugin.xivPluginLink.isReady() || this.settings?.channel == undefined) {
            this.renderInvalidState();
            return;
        }

        this.lastState = await FFXIVApi.Volume.getChannel(this.settings.channel);
        await this.render();
    }

    private renderInvalidState() {
        if (!this.isDial) {
            this.setState(0);
            return;
        }

        this.setFeedback({
            title: this.getChannelName(),
            value: "--",
            icon: "images/common/o_nodata.png",
            indicator: {
                value: 0,
                opacity: 0.6,
                bar_fill_c: null
            }
        });
    }

    private getChannelName() {
        if (this.settings?.channel == undefined) {
            return i18n.t("frames:volume.default-type");
        }

        let kn = `volumetypes:${this.settings.channel}.full`;
        return i18n.t(kn);
    }
    
    private getChannelShort(): string {
        if (this.settings?.channel == undefined) {
            return "--";
        }
        
        let kn = `volumetypes:${this.settings.channel}.short`;
        return i18n.t(kn);
    }

    private preEventGuard() {
        if (!plugin.xivPluginLink.isReady())
            throw new GameNotRunningError();

        if (!this.lastState)
            throw Error("Current volume state not loaded yet.");

        if (!this.settings?.channel)
            throw Error("Sound channel not yet set.");
    }
}
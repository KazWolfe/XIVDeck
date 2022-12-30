import {BaseButton} from "../BaseButton";
import { KeyDownEvent, TouchTapEvent, WillAppearEvent } from "@rweich/streamdeck-events/dist/Events/Received/Plugin";
import {
    DialRotateEvent,
    DialPressEvent
} from "@rweich/streamdeck-events/dist/Events/Received/Plugin/Dial";
import plugin from "../../plugin";
import {VolumeMessage, VolumePayload} from "../../link/ffxivplugin/GameTypes";
import {FFXIVApi} from "../../link/ffxivplugin/FFXIVApi";
import {FFXIVPluginLink} from "../../link/ffxivplugin/FFXIVPluginLink";
import {SetVolume} from "../../link/ffxivplugin/messages/outbound/SetVolume";
import i18n from "../../i18n/i18n";
import {DidReceiveSettingsEvent} from "@rweich/streamdeck-events/dist/Events/Received";

export type VolumeButtonSettings = {
    channel: string
    multiplier: number;
}

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
        this._sdEventListeners.set("keyDown", this.onPress.bind(this));
        this._sdEventListeners.set("dialPress", this.onPress.bind(this));
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
            delta: event.ticks * (this.settings?.multiplier ?? 1),
        }));
    }
    
    async onPress(event: DialPressEvent | KeyDownEvent | TouchTapEvent): Promise<void> {
        // ignore dial release events specifically
        if (event instanceof DialPressEvent && !event.pressed) return;
        
        this.preEventGuard();

        await FFXIVPluginLink.instance.send(new SetVolume(this.settings!.channel, {
            muted: !this.lastState!.muted
        }));
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
        
        if (!this.isDial) {
            this.setState(muted ? 1 : 0);
            return;
        }

        this.setFeedback({
            title: this.getChannelName(),
            value: muted ? "Muted" : this.lastState.volume,
            icon: `images/states/volume/o_${muted ? 'muted' : 'unmuted'}.png`,
            indicator: {
                value: this.lastState.volume,
                bar_fill_c: muted ? "red" : null
            }
        });
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
                value: 75,
                opacity: 0.6,
                bar_fill_c: null
            }
        });
    }

    private getChannelName() {
        if (this.settings?.channel == undefined) {
            return i18n.t("frames:volume.default-type");
        }

        let kn = `volumetypes:${this.settings.channel}`;
        return i18n.t(kn);
    }

    private preEventGuard() {
        if (!this.lastState) {
            throw Error("Current volume state not loaded yet.");
        }

        if (!this.settings?.channel) {
            throw Error("Sound channel not yet set.");
        }
    }
}
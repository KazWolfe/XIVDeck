import {BaseButton} from "../BaseButton";
import {KeyDownEvent, WillAppearEvent} from "@rweich/streamdeck-events/dist/Events/Received/Plugin";
import {
    TouchTapEvent,
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

    settings?: VolumeButtonSettings;
    lastState?: VolumePayload;

    constructor(event: WillAppearEvent) {
        super(event.context);
        
        this._xivEventListeners.add(plugin.xivPluginLink.on("_ready", this.loadFromGame.bind(this)));
        this._xivEventListeners.add(plugin.xivPluginLink.on("volumeUpdate", this.onVolumeUpdate.bind(this)));
        this._xivEventListeners.add(plugin.xivPluginLink.on("_wsClosed", this.renderInvalidState.bind(this)));
        
        this.onReceivedSettings(event);
    }
    
    async onReceivedSettings(event: DidReceiveSettingsEvent | WillAppearEvent) {
        this.settings = event.settings as VolumeButtonSettings;
        await this.loadFromGame();
    }

    async execute(event: KeyDownEvent | TouchTapEvent | DialRotateEvent | DialPressEvent): Promise<void> {
        // no-op
    }

    override async onDialPress(event: DialPressEvent): Promise<void> {
        // ignore dial release events
        if (!event.pressed) return;
        
        this.preEventGuard();

        await FFXIVPluginLink.instance.send(new SetVolume(this.settings!.channel, {
            muted: !this.lastState!.muted,
        }))
    }
    
    override async onKeyDown(event: KeyDownEvent): Promise<void> {
        this.preEventGuard();

        await FFXIVPluginLink.instance.send(new SetVolume(this.settings!.channel, {
            muted: !this.lastState!.muted,
        }))
    }

    override async onDialRotate(event: DialRotateEvent): Promise<void> {
        this.preEventGuard();

        let newVolume = this.lastState!.volume + event.ticks * (this.settings?.multiplier ?? 1);

        await FFXIVPluginLink.instance.send(new SetVolume(this.settings!.channel, {
            volume: newVolume
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
        
        this.setFeedback({
            title: this.getChannelName(),
            value: this.lastState?.muted ? "Muted" : this.lastState.volume,
            indicator: {
                value: this.lastState.volume,
                bar_fill_c: this.lastState.muted ? "red" : null
            }
        });
    }
    
    private async loadFromGame() {
        if (!plugin.xivPluginLink.isReady() || this.settings?.channel == undefined) {
            this.renderInvalidState();
            return;
        }

        this.lastState = await FFXIVApi.Volume.getChannel(this.settings.channel);
        this.render();
    }
    
    private renderInvalidState() {
        this.setFeedback({
            title: this.getChannelName(),
            value: "--",
            indicator: {
                value: 75,
                opacity: 0.6,
                bar_fill_c: null,
            }
        });
    }
    
    private getChannelName() {
        if (this.settings?.channel == undefined) {
            return i18n.t("frames:volume.default-type");
        }
        
        let kn = `volumetypes:${this.settings.channel}`
        return i18n.t(kn);
    }
    
    private preEventGuard() {
        if (!this.lastState) {
            throw Error("Current volume state not loaded yet.");
        }
        
        if (!this.settings?.channel) {
            throw Error("Sound channel not yet set.")
        }
    }
}
import {BaseButton} from "../BaseButton";
import AbstractStateEvent from "@rweich/streamdeck-events/dist/Events/Received/Plugin/AbstractStateEvent";
import {KeyDownEvent} from "@rweich/streamdeck-events/dist/Events/Received/Plugin";
import {
    TouchTapEvent,
    DialRotateEvent,
    DialPressEvent
} from "@rweich/streamdeck-events/dist/Events/Received/Plugin/Dial";
import plugin from "../../plugin";
import {VolumeMessage} from "../../link/ffxivplugin/GameTypes";
import {FFXIVApi} from "../../link/ffxivplugin/FFXIVApi";
import {FFXIVPluginLink} from "../../link/ffxivplugin/FFXIVPluginLink";
import {SetVolume} from "../../link/ffxivplugin/messages/outbound/SetVolume";
import i18n from "../../i18n/i18n";

export type VolumeButtonSettings = {
    channel: string
    multiplier: number;
}

export class VolumeButton extends BaseButton {
    useGameIcon: boolean = true;

    settings: VolumeButtonSettings;

    stateReceived: boolean = false;

    currentMuted: boolean = false;
    currentVolume: number = 0;

    constructor(event: AbstractStateEvent) {
        super(event.context);

        this.settings = event.settings as VolumeButtonSettings;
        
        this.loadFromGame();
        
        this._xivEventListeners.add(plugin.xivPluginLink.on("_ready", this.loadFromGame.bind(this)));
        this._xivEventListeners.add(plugin.xivPluginLink.on("volumeUpdate", this.volumeUpdate.bind(this)));
    }

    async execute(event: KeyDownEvent | TouchTapEvent | DialRotateEvent | DialPressEvent): Promise<void> {
        // no-op
    }

    override async onDialPress(event: DialPressEvent): Promise<void> {
        // ignore events before state init
        if (!this.stateReceived) return;

        // ignore dial release events
        if (!event.pressed) return;
        
        // ignore if unconfigured
        if (!this.settings.channel) return;

        await FFXIVPluginLink.instance.send(new SetVolume(this.settings.channel, {
            muted: !this.currentMuted,
        }))
    }

    override async onDialRotate(event: DialRotateEvent): Promise<void> {
        // ignore events before state init
        if (!this.stateReceived) return;

        // ignore if unconfigured
        if (!this.settings.channel) return;

        let newVolume = this.currentVolume + event.ticks * (this.settings.multiplier ?? 1);

        await FFXIVPluginLink.instance.send(new SetVolume(this.settings.channel, {
            volume: newVolume
        }));
    }

    async volumeUpdate(message: VolumeMessage): Promise<void> {
        if (message.channel != this.settings.channel) return;
        
        this.stateReceived = true;
        
        this.currentMuted = message.data.muted ?? false;
        this.currentVolume = message.data.volume ?? 0;

        await this.render();
    }

    async render() {
        if (this.settings.channel == null || !this.stateReceived) {
            this.renderInvalidState();
            return;
        }
        
        this.setFeedback({
            title: this.getChannelName(),
            value: this.currentMuted ? "Muted" : this.currentVolume,
            indicator: {
                value: this.currentVolume,
                bar_fill_c: this.currentMuted ? "red" : null
            }
        });
    }
    
    private async loadFromGame() {
        if (!plugin.xivPluginLink.isReady() || this.settings.channel == null) {
            this.renderInvalidState();
            return;
        }
        
        let payload = await FFXIVApi.Volume.getChannel(this.settings.channel);
        
        this.stateReceived = true;
        this.currentVolume = payload.volume;
        this.currentMuted = payload.muted;
        
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
        if (this.settings.channel == null) {
            return i18n.t("frames:volume.default-type");
        }
        
        let kn = `volumetypes:${this.settings.channel}`
        return i18n.t(kn);
    }
}
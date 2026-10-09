import {injectable} from "inversify";
import {DialAction, DialDownEvent, DialRotateEvent, TouchTapEvent} from "@elgato/streamdeck";
import {JsonObject} from "@elgato/utils";
import {IDialControl} from "#/control/ControlInput";
import {ControlContext} from "#/control/ControlContext";
import {ClientProxy} from "#/control/proxy/ClientProxy";
import {GameNotificationProxy} from "#/control/proxy/GameNotificationProxy";
import {VolumeControlBase} from "./VolumeControlBase";
import {VolumeControlSettings} from "#/settings/types/VolumeControlSettings";
import {VolumeState} from "#/client/rpc/messages/Volume";
import i18n from "#/i18n/i18n";

@injectable()
export class VolumeDialControl extends VolumeControlBase implements IDialControl {
    private static readonly DEFAULT_STEP = 2;
    private static readonly MIN_STEP = 1;
    private static readonly MAX_STEP = 5;

    private readonly _action: DialAction<VolumeControlSettings>;

    public constructor(context: ControlContext, client: ClientProxy, notifications: GameNotificationProxy) {
        super(context, client, notifications);
        this._action = context.asDial<VolumeControlSettings>();
    }

    public async onDialRotate(ev: DialRotateEvent<JsonObject>): Promise<void> {
        const settings = this.tryReadSeconds();

        await this.applyState(settings, await this.client.request("Volume.DeltaChannel", {
            channel: settings.channel, delta: ev.payload.ticks * this.getStep(settings),
        }));
    }

    public async onDialDown(_ev: DialDownEvent<JsonObject>): Promise<void> {
        await this.toggleMute();
    }

    public async onTouchTap(_ev: TouchTapEvent<JsonObject>): Promise<void> {
        await this.toggleMute();
    }

    protected async renderNoData(): Promise<void> {
        await this._action.setFeedback({
            value: "--",
            icon: "images/common/o_nodata.svg",
            indicator: {value: 0, opacity: 0.6},
        });
    }

    protected async applyState(settings: VolumeControlSettings, state: VolumeState): Promise<void> {
        await this._action.setFeedback({
            title: i18n.t(`soundChannels:${settings.channel}.full`),
            value: state.muted ? i18n.t("controls:VolumeControl.muted") : state.volume,
            icon: `images/states/volume/${state.muted ? "muted" : "unmuted"}_o.svg`,
            indicator: {value: state.volume, opacity: 1.0, bar_fill_c: state.muted ? "red" : "white"},
        });
    }

    private async toggleMute(): Promise<void> {
        const settings = this.tryReadSeconds();
        await this.applyState(settings, await this.client.request("Volume.ToggleMuteChannel", {channel: settings.channel}));
    }

    private getStep(settings: VolumeControlSettings): number {
        const step = settings.multiplier;
        if (typeof step !== "number" || step <= 0) {
            return VolumeDialControl.DEFAULT_STEP;
        }

        return Math.min(VolumeDialControl.MAX_STEP, Math.max(VolumeDialControl.MIN_STEP, Math.round(step)));
    }
}

import {injectable} from "inversify";
import {KeyAction, KeyDownEvent} from "@elgato/streamdeck";
import {JsonObject} from "@elgato/utils";
import {IKeyControl} from "#/control/ControlInput";
import {ControlContext} from "#/control/ControlContext";
import {ClientProxy} from "#/control/proxy/ClientProxy";
import {GameNotificationProxy} from "#/control/proxy/GameNotificationProxy";
import {VolumeControlBase} from "./VolumeControlBase";
import {VolumeControlMode, VolumeControlSettings} from "#/settings/types/VolumeControlSettings";
import {VolumeState} from "#/client/rpc/messages/Volume";
import i18n from "#/i18n/i18n";

@injectable()
export class VolumeKeyControl extends VolumeControlBase implements IKeyControl {
    private readonly _action: KeyAction<VolumeControlSettings>;

    public constructor(context: ControlContext, client: ClientProxy, notifications: GameNotificationProxy) {
        super(context, client, notifications);
        this._action = context.asKey<VolumeControlSettings>();
    }

    public async onKeyDown(_ev: KeyDownEvent<JsonObject>): Promise<void> {
        const settings = this.tryReadSeconds();

        switch (settings.mode) {
            case VolumeControlMode.SET:
                await this.applyState(settings, await this.client.request("Volume.SetChannel", {
                    channel: settings.channel, volume: settings.value,
                }));
                break;

            case VolumeControlMode.ADJUST:
                await this.applyState(settings, await this.client.request("Volume.DeltaChannel", {
                    channel: settings.channel, delta: settings.multiplier,
                }));
                break;

            default:
                await this.applyState(settings, await this.client.request("Volume.ToggleMuteChannel", {
                    channel: settings.channel,
                }));
        }
    }

    protected async renderNoData(): Promise<void> {
        await this._action.setState(0);
    }

    protected async applyState(settings: VolumeControlSettings, state: VolumeState): Promise<void> {
        const shortName = i18n.t(`soundChannels:${settings.channel}.short`);

        switch (settings.mode) {
            case VolumeControlMode.SET:
                await this._action.setTitle(`${shortName} ${settings.value}`);
                break;
            case VolumeControlMode.ADJUST:
                await this._action.setTitle(`${shortName} ${settings.multiplier >= 0 ? "+" : ""}${settings.multiplier}`);
                break;
            default:
                await this._action.setTitle(shortName);
        }

        await this._action.setState(state.muted ? 1 : 0);
    }
}

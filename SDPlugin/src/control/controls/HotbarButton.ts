import {injectable} from "inversify";
import {KeyAction, KeyDownEvent} from "@elgato/streamdeck";
import {JsonObject} from "@elgato/utils";
import {Control} from "#/control/Control";
import {IKeyControl} from "#/control/ControlInput";
import {ControlContext} from "#/control/ControlContext";
import {ClientProxy} from "#/control/proxy/ClientProxy";
import {HotbarTrackerProxy} from "#/control/proxy/HotbarTrackerProxy";
import {VirtualSlotPresenter} from "#/control/virtual_slot/VirtualSlotPresenter";
import {HotbarButtonSettings} from "#/settings/types/HotbarButtonSettings";

@injectable()
export class HotbarButton extends Control<HotbarButtonSettings> implements IKeyControl {
    private readonly _action: KeyAction<HotbarButtonSettings>;

    public constructor(context: ControlContext, client: ClientProxy, private readonly slot: VirtualSlotPresenter,
                       private readonly hotbars: HotbarTrackerProxy) {
        super(context, client);
        this._action = context.asKey<HotbarButtonSettings>();

        slot.setPresentHandler(this.presentSlot.bind(this));
        hotbars.changed.add(this.requestRender.bind(this));
    }

    public async onKeyDown(_ev: KeyDownEvent<JsonObject>): Promise<void> {
        const {hotbarId, slotId} = this.tryReadSeconds();
        await this.client.request("Hotbar.TriggerHotbarSlot", {hotbarId, slotId});
    }

    protected override onSettingsChanged(_previous: HotbarButtonSettings | undefined): void {
        if (this.settings) {
            this.hotbars.watch({hotbarId: this.settings.hotbarId, slotId: this.settings.slotId});
        } else {
            this.hotbars.release();
        }
    }

    protected async render(): Promise<void> {
        const settings = this.settings;
        if (!settings) {
            this.slot.clear();
            return;
        }

        if (!this.client.isAvailable) {
            return;
        }

        const {hotbarId, slotId} = settings;
        await this.slot.setAppearance(await this.client.request("Hotbar.GetHotbarSlot", {hotbarId, slotId}));
    }

    private async presentSlot(image: string): Promise<void> {
        await this._action.setImage(image);
    }
}

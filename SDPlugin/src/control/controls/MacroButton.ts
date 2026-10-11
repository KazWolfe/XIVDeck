import {injectable} from "inversify";
import {KeyAction, KeyDownEvent} from "@elgato/streamdeck";
import {JsonObject} from "@elgato/utils";
import {Control} from "#/control/Control";
import {IKeyControl} from "#/control/ControlInput";
import {ControlContext} from "#/control/ControlContext";
import {ActionTypeUpdates} from "#/control/ActionTypeUpdates";
import {ClientProxy} from "#/control/proxy/ClientProxy";
import {GameNotificationProxy} from "#/control/proxy/GameNotificationProxy";
import {VirtualSlotPresenter} from "#/control/virtual_slot/VirtualSlotPresenter";
import {MacroButtonSettings} from "#/settings/types/MacroButtonSettings";
import {ActionTypeUpdateBatch} from "#/client/rpc/messages/Action";
import MACRO_TYPE_ICON_SVG from "../../../assets/templates/MacroTypeIcon.svg";

@injectable()
export class MacroButton extends Control<MacroButtonSettings> implements IKeyControl {
    private readonly _action: KeyAction<MacroButtonSettings>;

    public constructor(context: ControlContext, client: ClientProxy, private readonly slot: VirtualSlotPresenter,
                       notifications: GameNotificationProxy) {
        super(context, client);
        this._action = context.asKey<MacroButtonSettings>();

        slot.setPresentHandler(this.presentSlot.bind(this));
        notifications.on("Action.ActionTypeUpdate", this.onActionTypeUpdate.bind(this));
    }

    public async onKeyDown(_ev: KeyDownEvent<JsonObject>): Promise<void> {
        await this.client.request("Action.ExecuteAction", {type: "Macro", id: this.tryReadSeconds().macroId});
    }

    protected async render(): Promise<void> {
        const settings = this.settings;
        if (!settings) {
            this.slot.clear();
            return;
        }

        if (!this.client.isAvailable) {
            await this.slot.reset();
            return;
        }

        const appearance = await this.client.request("Action.GetActionAppearance", {type: "Macro", id: settings.macroId});
        await this.slot.setAppearance(appearance, MACRO_TYPE_ICON_SVG);
    }

    private async onActionTypeUpdate(batch: ActionTypeUpdateBatch): Promise<void> {
        const settings = this.settings;
        if (settings && ActionTypeUpdates.affects(batch, "Macro", settings.macroId)) {
            await this.requestRender();
        }
    }

    private async presentSlot(image: string | null): Promise<void> {
        await this._action.setImage(image ?? undefined);
    }
}

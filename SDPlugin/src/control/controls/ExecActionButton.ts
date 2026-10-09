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
import {ExecActionSettings} from "#/settings/types/ExecActionSettings";
import {ActionTypeUpdateBatch} from "#/client/rpc/messages/Action";

@injectable()
export class ExecActionButton extends Control<ExecActionSettings> implements IKeyControl {
    private readonly _action: KeyAction<ExecActionSettings>;

    public constructor(context: ControlContext, client: ClientProxy, private readonly slot: VirtualSlotPresenter,
                       notifications: GameNotificationProxy) {
        super(context, client);
        this._action = context.asKey<ExecActionSettings>();

        slot.setPresentHandler(this.presentSlot.bind(this));
        notifications.on("Action.ActionTypeUpdate", this.onActionTypeUpdate.bind(this));
    }

    public async onKeyDown(_ev: KeyDownEvent<JsonObject>): Promise<void> {
        const {actionType, actionId, payload} = this.tryReadSeconds();
        await this.client.request("Action.ExecuteAction", {type: actionType, id: actionId, payload: payload ?? null});
    }

    public override async handlePiRequest(command: string, params: unknown): Promise<unknown> {
        if (command !== "listActions") {
            return super.handlePiRequest(command, params);
        }

        return (await this.client.request("Action.GetActions")).actions;
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

        const {actionType, actionId} = settings;

        // cache the button's action data just so the PI can stay up to date.
        // called here since some actions (gearsets especially) can update outside the PI's lifecycle.
        const entry = await this.client.request("Action.GetActionEntry", {type: actionType, id: actionId});
        await this.saveSettings({...settings, cache: entry});

        await this.slot.setAppearance(await this.client.request("Action.GetActionAppearance", {type: actionType, id: actionId}));
    }

    private async onActionTypeUpdate(batch: ActionTypeUpdateBatch): Promise<void> {
        const settings = this.settings;
        if (settings && ActionTypeUpdates.affects(batch, settings.actionType, settings.actionId)) {
            await this.requestRender();
        }
    }

    private async presentSlot(image: string): Promise<void> {
        await this._action.setImage(image);
    }
}

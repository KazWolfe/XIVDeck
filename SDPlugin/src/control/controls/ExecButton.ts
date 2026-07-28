import {KeyDownEvent, WillAppearEvent} from "@elgato/streamdeck";
import {VirtualSlotControl} from "../VirtualSlotControl";
import {ClientProvider} from "../../rpc/ClientProvider";
import {ExecActionSettings} from "../../settings/types/ExecActionSettings";
import {ActionTypeUpdateBatch} from "../../rpc/messages/Action";
import {GlobalSettingsProvider} from "../../settings/GlobalSettingsProvider";

export class ExecButton extends VirtualSlotControl<ExecActionSettings> {
    constructor(ev: WillAppearEvent<ExecActionSettings>, clients: ClientProvider, globalSettings: GlobalSettingsProvider) {
        super(ev, clients, "execAction", globalSettings);

        this.onClientEvent("Action.ActionTypeUpdate", batch => this._onActionTypeUpdate(batch));
    }

    protected async render(): Promise<void> {
        const client = this.activeClient;
        const settings = this.settings;

        if (!client.isReady() || !settings) {
            return;
        }

        const {actionType, actionId} = settings;
        const entry = await client.request("Action.GetActionEntry", {type: actionType, id: actionId});

        // update our cached action info
        if (JSON.stringify(entry) !== JSON.stringify(settings.cache)) {
            this.settings = {...settings, cache: entry};
            await this.action.setSettings(this.settings);
        }

        const appearance = await client.request("Action.GetActionAppearance", {type: actionType, id: actionId});
        await this.applyAppearance(appearance);
    }

    async onKeyDown(_ev: KeyDownEvent<ExecActionSettings>): Promise<void> {
        if (!this.settings) {
            throw new Error("No action type/ID configured for this button.");
        }

        await this.activeClient.request("Action.ExecuteAction", {
            type: this.settings.actionType,
            id: this.settings.actionId,
            payload: this.settings.payload ?? null,
        });
    }

    async handlePiRequest(command: string, params: unknown): Promise<unknown> {
        if (command !== "listActions") {
            return super.handlePiRequest(command, params);
        }

        return (await this.activeClient.request("Action.GetActions")).actions;
    }

    private async _onActionTypeUpdate(batch: ActionTypeUpdateBatch): Promise<void> {
        const settings = this.settings;
        if (!settings) return;

        const isRelevant = batch.updates.some((notification) =>
            settings.actionType === notification.updatedType &&
            (notification.actionId == null || notification.actionId === settings.actionId),
        );
        if (!isRelevant) return;

        await this.safeRender();
    }
}

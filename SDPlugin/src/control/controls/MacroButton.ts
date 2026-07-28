import {KeyDownEvent, WillAppearEvent} from "@elgato/streamdeck";
import {VirtualSlotControl} from "../VirtualSlotControl";
import {ClientProvider} from "../../rpc/ClientProvider";
import {MacroButtonSettings} from "../../settings/types/MacroButtonSettings";
import {ActionTypeUpdateBatch} from "../../rpc/messages/Action";
import {ResolvedActionIcons} from "../../render/HotbarDisplay";
import {GlobalSettingsProvider} from "../../settings/GlobalSettingsProvider";
import MACRO_TYPE_ICON_SVG from "../../../assets/templates/MacroTypeIcon.svg";

export class MacroButton extends VirtualSlotControl<MacroButtonSettings> {
    constructor(ev: WillAppearEvent<MacroButtonSettings>, clients: ClientProvider, globalSettings: GlobalSettingsProvider) {
        super(ev, clients, "macro", globalSettings);

        this.onClientEvent("Action.ActionTypeUpdate", batch => this._onActionTypeUpdate(batch));
    }

    protected async render(): Promise<void> {
        const client = this.activeClient;
        const settings = this.settings;

        if (!client.isReady() || !settings) {
            return;
        }

        const appearance = await client.request("Action.GetActionAppearance", {
            type: "Macro",
            id: settings.macroId,
        });

        await this.applyAppearance(appearance);
    }

    async onKeyDown(_ev: KeyDownEvent<MacroButtonSettings>): Promise<void> {
        if (!this.settings) {
            throw new Error("No macro ID configured for this button.");
        }

        await this.activeClient.request("Action.ExecuteAction", {type: "Macro", id: this.settings.macroId});
    }

    protected extraIcons(): Omit<ResolvedActionIcons, "iconBase64"> {
        return {typeIconSvg: MACRO_TYPE_ICON_SVG};
    }

    private async _onActionTypeUpdate(batch: ActionTypeUpdateBatch): Promise<void> {
        if (!batch.updates.some((notification) => notification.updatedType === "Macro")) return;

        await this.safeRender();
    }
}

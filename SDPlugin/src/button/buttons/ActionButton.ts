import { BaseButton } from "../BaseButton";
import { KeyDownEvent, WillAppearEvent } from "@rweich/streamdeck-events/dist/Events/Received/Plugin";
import plugin from "../../plugin";
import { FFXIVAction, StateMessage } from "../../link/ffxivplugin/GameTypes";
import { FFXIVApi } from "../../link/ffxivplugin/FFXIVApi";
import { DidReceiveSettingsEvent } from "@rweich/streamdeck-events/dist/Events/Received";

export type ActionButtonSettings = {
    actionType?: string,
    actionId?: number,
    actionName?: string,
    cache?: FFXIVAction
    payload?: unknown
}

export class ActionButton extends BaseButton {
    settings?: ActionButtonSettings;

    useGameIcon: boolean = true;

    constructor(event: WillAppearEvent) {
        super(event.context);

        this._xivEventListeners.add(plugin.xivPluginLink.on("_ready", this.render.bind(this)));
        this._xivEventListeners.add(plugin.xivPluginLink.on("stateUpdate", this.stateUpdate.bind(this)));

        this._sdEventListeners.set("keyDown", this.onKeyDown.bind(this));

        this.onReceivedSettings(event);
    }

    async onReceivedSettings(event: DidReceiveSettingsEvent | WillAppearEvent): Promise<void> {
        this.settings = event.settings as ActionButtonSettings;
        await this.render();
    }

    async onKeyDown(event: KeyDownEvent): Promise<void> {
        if (this.settings?.actionType == undefined || this.settings?.actionId == undefined) {
            throw Error("Not action type/ID was defined for this button!");
        }

        await FFXIVApi.Action.executeAction(this.settings.actionType, this.settings.actionId, this.settings.payload);
    }

    async render() {
        if (!this.useGameIcon) {
            this.setImage("");
            return;
        }

        if (!plugin.xivPluginLink.isReady()) {
            return;
        }

        if (this.settings?.actionType == undefined || this.settings?.actionId == undefined) {
            return;
        }

        // Migration again.
        // TODO: Remove eventually.
        if (this.settings.actionType == "Collection") {
            this.settings.actionType = "McGuffin";
            this.setSettings(this.settings);
        }

        let actionInfo = await FFXIVApi.Action.getAction(this.settings.actionType, this.settings.actionId);
        this.setImage(await FFXIVApi.getIcon(actionInfo.iconId));

        // Populate cache if it isn't already set
        if (this.settings.cache == null) {
            this.settings.cache = actionInfo;

            // ToDo: Remove this migration eventually
            this.settings.actionName = undefined;
            console.log("MIGRATION PERFORMED!!");

            this.setSettings(this.settings);
        }

    }

    private stateUpdate(message: StateMessage) {
        if (this.settings?.actionType == "GearSet" && message.type == "GearSet") {
            this.render();
        }
    }
}

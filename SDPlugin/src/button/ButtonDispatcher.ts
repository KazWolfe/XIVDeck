import {WillAppearEvent, WillDisappearEvent} from "@rweich/streamdeck-events/dist/Events/Received/Plugin";
import { GameNotRunningError } from "../link/ffxivplugin/exceptions/Exceptions";
import {BaseButton} from "./BaseButton";
import {ActionButton} from "./buttons/ActionButton";
import {HotbarButton} from "./buttons/HotbarButton";
import {CommandButton} from "./buttons/CommandButton";
import {MacroButton} from "./buttons/MacroButton";
import plugin from "../plugin";
import {DidReceiveSettingsEvent} from "@rweich/streamdeck-events/dist/Events/Received";
import {ClassButton} from "./buttons/ClassButton";
import {StateMessage} from "../link/ffxivplugin/GameTypes";
import {FFXIVPluginLink} from "../link/ffxivplugin/FFXIVPluginLink";
import {VolumeButton} from "./buttons/VolumeButton";
import {InteractiveEvent} from "../util/SDEvent";

export class ButtonDispatcher {
    private _contextCache: Map<string, BaseButton> = new Map<string, BaseButton>();

    constructor() {
        FFXIVPluginLink.instance.on("stateUpdate", this._globalStateUpdate.bind(this));
    }

    private _constructButton(event: WillAppearEvent): BaseButton {
        let button: BaseButton;

        switch (event.action.toLowerCase()) {
            case "dev.wolf.xivdeck.sdplugin.actions.sendcommand":
                button = new CommandButton(event);
                break;
            case "dev.wolf.xivdeck.sdplugin.actions.exechotbar":
                button = new HotbarButton(event);
                break;
            case "dev.wolf.xivdeck.sdplugin.actions.execaction":
                button = new ActionButton(event);
                break;
            case "dev.wolf.xivdeck.sdplugin.actions.execmacro":
                button = new MacroButton(event);
                break;
            case "dev.wolf.xivdeck.sdplugin.actions.switchclass":
                button = new ClassButton(event);
                break;
            case "dev.wolf.xivdeck.sdplugin.actions.volume":
                button = new VolumeButton(event);
                break;
            default:
                throw new Error(`Undefined action type: ${event.action}`)
        }

        this._contextCache.set(event.context, button);

        // can usually be ignored, but useful every now and then
        return button;
    }

    private _destructButton(context: string) {
        let button = this._contextCache.get(context);

        if (button == null) {
            console.debug(`Couldn't delete button with context ${context} as it doesnt exist in cache.`)
            return
        }

        button.cleanup();
        this._contextCache.delete(context);
    }

    private async _globalStateUpdate(event: StateMessage) {
        switch (event.type) {
            case "DEBUG_ClearIcons":
                for await (let c of this._contextCache.values()) {
                    await c.setImage("")
                }
                break;
            case "IconCache":
                for await (let c of this._contextCache.values()) {
                    await c.render()
                }
                break;
        }
    }

    public dispatch(event: InteractiveEvent) {
        let button = this._contextCache.get(event.context);

        if (!button) {
            plugin.sdPluginLink.showAlert(event.context);
            throw Error("Somehow got a button that wasn't in cache!");
        }

        button.dispatch(event)
            .catch((e) => {
                button!.showAlert();

                if (e instanceof GameNotRunningError) {
                    console.warn("Could not process event as the game is not running.", event)
                } else {
                    console.error("Error trying to execute button:", e, event);
                    plugin.sdPluginLink.logMessage(`Got error while trying to execute button: ${e}`);
                }
            });
    }

    handleWillAppear(event: WillAppearEvent): void {
        // bust the icon cache for test/debug purposes
        // plugin.sdPluginLink.setImage("", event.context);

        this._constructButton(event);
    }

    handleReceivedSettings(event: DidReceiveSettingsEvent) {
        let button = this._contextCache.get(event.context);

        if (button == null) {
            throw new Error(`No button with context ${event.context} was found in cache!`);
        }

        button.onReceivedSettings(event);
    }

    handleWillDisappear(event: WillDisappearEvent) {
        // delete the context cache entry as it's no longer necessary
        this._destructButton(event.context);
    }
}

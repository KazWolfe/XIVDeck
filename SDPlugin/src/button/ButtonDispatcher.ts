import {KeyDownEvent, WillAppearEvent, WillDisappearEvent} from "@rweich/streamdeck-events/dist/Events/Received/Plugin";
import {BaseButton} from "./BaseButton";
import {ActionButton} from "./buttons/ActionButton";
import {HotbarButton} from "./buttons/HotbarButton";
import {CommandButton} from "./buttons/CommandButton";
import {MacroButton} from "./buttons/MacroButton";
import plugin from "../plugin";
import {DidReceiveSettingsEvent} from "@rweich/streamdeck-events/dist/Events/Received";
import AbstractStateEvent from "@rweich/streamdeck-events/dist/Events/Received/Plugin/AbstractStateEvent";
import {ClassButton} from "./buttons/ClassButton";

export class ButtonDispatcher {
    private _contextCache: Map<string, BaseButton> = new Map<string, BaseButton>();
    
    private _constructButton(event: AbstractStateEvent): BaseButton {
        let button: BaseButton;

        switch (event.action) {
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
    
    handleWillAppear(event: WillAppearEvent): void {
        // bust the icon cache for test/debug purposes
        // plugin.sdPluginLink.setImage("", event.context);
        
        this._constructButton(event);
    }
    
    handleReceivedSettings(event: DidReceiveSettingsEvent) {
        // the "simple" way of doing this is honestly to just delete it from cache and start fresh.
        // this may trigger a render call, but that's honestly fine as we'd normally have to anyways.
        this._destructButton(event.context);
        this._constructButton(event);
    }
    
    handleWillDisappear(event: WillDisappearEvent) {
        // delete the context cache entry as it's no longer necessary
        this._destructButton(event.context);
    }
    
    handleKeyDown(event: KeyDownEvent) {
        let button = this._contextCache.get(event.context);
        
        if (!button) {
            plugin.sdPluginLink.showAlert(event.context);
            throw Error("Somehow got a button that wasn't in cache!");
        }
        
        button.execute(event)
            .catch((e) => {
                button!.showAlert();
                console.error("Error trying to execute button:", e, event);
            });
    }
}
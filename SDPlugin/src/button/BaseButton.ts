import {KeyDownEvent} from "@rweich/streamdeck-events/dist/Events/Received/Plugin";
import plugin from "../plugin";
import {FFXIVOpcode} from "../link/ffxivplugin/MessageBase";
import {FFXIVGenericResponse} from "../link/ffxivplugin/GameTypes";

type SetterTargets = 'hardware' | 'software' | 'both';

export abstract class BaseButton {
    public context: string;
    
    // set of registered XIV events bound to this button
    protected _xivEventListeners: Set<Function | undefined> = new Set<Function | undefined>();

    protected constructor(context: string) {
        this.context = context;
    }
    
    abstract execute(event: KeyDownEvent): Promise<void>;
    
    
    // cleanup tasks, if any, can be specified by overriding this particular method
    public cleanup() {
        this._xivEventListeners.forEach((eventDeleter) => {
            if (eventDeleter) eventDeleter();
        });
    }
    
    /* execute wrapper, stylistic */
    protected async _sendExec(message: FFXIVOpcode): Promise<void> {
        await plugin.xivPluginLink.sendExpectingGeneric(message);
    }
    
    
    /* wrappers for the exposed elgato api, except with built-in context sensitivity */
    
    public showAlert(): void {
        plugin.sdPluginLink.showAlert(this.context);
    }
    
    public showOk(): void {
        plugin.sdPluginLink.showOk(this.context);
    }
    
    public setImage(imageData: string, options: { target?: SetterTargets; state?: number }= {}): void {
        plugin.sdPluginLink.setImage(imageData, this.context, options);
    }
    
    public setTitle(title: string, options: { target?: SetterTargets; state?: number } = {}): void {
        plugin.sdPluginLink.setTitle(title, this.context, options);
    }
}

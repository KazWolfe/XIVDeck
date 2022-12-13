import {DidReceiveSettingsEvent} from "@rweich/streamdeck-events/dist/Events/Received";
import {WillAppearEvent} from "@rweich/streamdeck-events/dist/Events/Received/Plugin";
import {EventsSent} from "@rweich/streamdeck-events";
import {LayoutFeedback} from "@rweich/streamdeck-events/dist/StreamdeckTypes/Received/Feedback/LayoutFeedback";

import plugin from "../plugin";
import { InteractiveEvent } from "../util/SDEvent";

type SetterTargets = 'hardware' | 'software' | 'both';

export abstract class BaseButton {
    public context: string;
    
    // set of registered XIV events bound to this button
    protected _xivEventListeners: Set<Function | undefined> = new Set<Function | undefined>();
    
    // set of event listeners
    protected _sdEventListeners: Map<InteractiveEvent['event'], Function> = new Map<InteractiveEvent['event'], Function>();

    protected constructor(context: string) {
        this.context = context;
    }
    
    abstract onReceivedSettings(event: DidReceiveSettingsEvent | WillAppearEvent): Promise<void>;
    abstract render(): Promise<void>;
    
    // cleanup tasks, if any, can be specified by overriding this particular method
    public cleanup() {
        this._xivEventListeners.forEach((eventDeleter) => {
            if (eventDeleter) eventDeleter();
        });
        
        this._sdEventListeners.clear();
    }
    
    public dispatch(event: InteractiveEvent): Promise<void> {
        let eventHandler = this._sdEventListeners.get(event.event);
        if (eventHandler) {
            return eventHandler(event);
        }
        
        // no event handler exists for this, just suppress
        return Promise.resolve();
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
    
    public setState(state: number) {
        plugin.sdPluginLink.setState(state, this.context);
    }
    
    public setFeedback(payload: LayoutFeedback) {
        var sentEventFactory: EventsSent = (<any> plugin.sdPluginLink).sentEventFactory;
        
        (<any> plugin.sdPluginLink).sendToStreamdeck(sentEventFactory.setFeedback(payload, this.context));
    }
}

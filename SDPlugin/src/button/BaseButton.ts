import {
    DialPressEvent, DialRotateEvent, TouchTapEvent
} from "@rweich/streamdeck-events/dist/Events/Received/Plugin/Dial";
import {KeyDownEvent} from "@rweich/streamdeck-events/dist/Events/Received/Plugin";
import {EventsSent} from "@rweich/streamdeck-events";
import {LayoutFeedback} from "@rweich/streamdeck-events/dist/StreamdeckTypes/Received/Feedback/LayoutFeedback";

import plugin from "../plugin";

type SetterTargets = 'hardware' | 'software' | 'both';

export abstract class BaseButton {
    public context: string;
    
    // set of registered XIV events bound to this button
    protected _xivEventListeners: Set<Function | undefined> = new Set<Function | undefined>();

    protected constructor(context: string) {
        this.context = context;
    }
    
    abstract execute(event: any): Promise<void>;
    abstract render(): Promise<void>;
    
    // cleanup tasks, if any, can be specified by overriding this particular method
    public cleanup() {
        this._xivEventListeners.forEach((eventDeleter) => {
            if (eventDeleter) eventDeleter();
        });
    }
    
    public dispatch(event: any): Promise<void> {
        if (event instanceof TouchTapEvent) {
            return this.onTouchTap(event);
        } else if (event instanceof DialRotateEvent) {
            return this.onDialRotate(event);
        } else if (event instanceof DialPressEvent) {
            return this.onDialPress(event);
        } 
        
        return this.execute(event);
    }
    
    public onTouchTap(event: TouchTapEvent): Promise<void> {
        return this.execute(event);
    }
    
    public onDialRotate(event: DialRotateEvent): Promise<void> {
        return this.execute(event);
    }
    
    public onDialPress(event: DialPressEvent): Promise<void> {
        return this.execute(event);
    }
    
    public onKeyDown(event: KeyDownEvent): Promise<void> {
        return this.execute(event);
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
    
    public setFeedback(payload: LayoutFeedback) {
        var sentEventFactory: EventsSent = (<any> plugin.sdPluginLink).sentEventFactory;
        
        (<any> plugin.sdPluginLink).sendToStreamdeck(sentEventFactory.setFeedback(payload, this.context));
    }
}

import {BaseButton} from "../BaseButton";
import {KeyDownEvent, WillAppearEvent} from "@rweich/streamdeck-events/dist/Events/Received/Plugin";
import plugin from "../../plugin";
import {FFXIVApi} from "../../link/ffxivplugin/FFXIVApi";
import {DidReceiveSettingsEvent} from "@rweich/streamdeck-events/dist/Events/Received";

export type ClassButtonSettings = {
    classId: number,
    className: string
}

export class ClassButton extends BaseButton {
    useGameIcon: boolean = true;
    
    settings?: ClassButtonSettings;

    constructor(event: WillAppearEvent) {
        super(event.context);
        
        this._xivEventListeners.add(plugin.xivPluginLink.on("_ready", this.render.bind(this)));
        
        this._sdEventListeners.set("keyDown", this.onKeyDown.bind(this));
        
        this.onReceivedSettings(event);
    }
    
    async onReceivedSettings(event: DidReceiveSettingsEvent | WillAppearEvent) {
        this.settings = event.settings as ClassButtonSettings;
        this.render();
    }
    
    async onKeyDown(event: KeyDownEvent): Promise<void> {
        if (this.settings?.classId == undefined) {
            throw new Error("No class specified for this button");
        }
        
        await FFXIVApi.GameClass.triggerClass(this.settings.classId);
    }
    
    async render(): Promise<void> {
        if (!this.useGameIcon) {
            this.setImage("");
            return
        }

        if (!plugin.xivPluginLink.isReady()) {
            return;
        }

        if (this.settings?.classId == undefined) {
            return
        }
        
        let classInfo = await FFXIVApi.GameClass.getClass(this.settings.classId);
        this.setImage(await FFXIVApi.getIcon(classInfo.iconId));
    }
}
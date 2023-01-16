import { DialRotateEvent } from "@rweich/streamdeck-events/dist/Events/Received/Plugin/Dial";
import { LayoutA1Feedback } from "@rweich/streamdeck-events/dist/StreamdeckTypes/Received/Feedback/LayoutFeedback";
import { FFXIVClass, StateMessage } from "../../link/ffxivplugin/GameTypes";
import { NumberUtils } from "../../util/NumberUtils";
import { StringUtils } from "../../util/StringUtils";
import {BaseButton} from "../BaseButton";
import { DialPressEvent, KeyDownEvent, WillAppearEvent } from "@rweich/streamdeck-events/dist/Events/Received/Plugin";
import plugin from "../../plugin";
import {FFXIVApi} from "../../link/ffxivplugin/FFXIVApi";
import {DidReceiveSettingsEvent} from "@rweich/streamdeck-events/dist/Events/Received";

export type ClassButtonSettings = {
    classId: number,
    className: string
}

export class ClassButton extends BaseButton {
    isDial: boolean;
    
    settings?: ClassButtonSettings;
    
    // dial things
    classCache?: FFXIVClass[];
    selectedClassIndex: number = 0;

    constructor(event: WillAppearEvent) {
        super(event.context);
        
        this.isDial = (event.controller == "Encoder");
        
        this._xivEventListeners.add(plugin.xivPluginLink.on("_ready", this.render.bind(this)));
        this._xivEventListeners.add(plugin.xivPluginLink.on("stateUpdate", this.stateUpdate.bind(this)));
        
        this._sdEventListeners.set("keyDown", this.onKeyDown.bind(this));
        
        this._sdEventListeners.set("dialRotate", this.onDialRotate.bind(this));
        this._sdEventListeners.set("dialPress", this.onDialPress.bind(this));
        
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
    
    async onDialPress(event: DialPressEvent): Promise<void> {
        if (!this.classCache || !this.selectedClassIndex) return;
        
        await FFXIVApi.GameClass.triggerClass(this.classCache[this.selectedClassIndex].id);
    }
    
    async onDialRotate(event: DialRotateEvent): Promise<void> {
        if (!this.classCache) return;
        
        this.selectedClassIndex = NumberUtils.mod(this.selectedClassIndex + event.ticks, this.classCache.length);
        await this._renderDial();
    }
    
    async render(): Promise<void> {
        if (!plugin.xivPluginLink.isReady()) {
            return;
        }

        if (this.isDial) {
            await this._updateClassCache();
            await this._renderDial();
        } else {
            await this._renderButton();
        }
    }
    
    async stateUpdate(message: StateMessage): Promise<void> {
        if (message.type != "GearSet" || !this.isDial) return;
        await this._updateClassCache();
        await this._renderDial();
    }
    
    private async _renderButton(): Promise<void> {
        if (this.settings?.classId == undefined) {
            return
        }

        let classInfo = await FFXIVApi.GameClass.getClass(this.settings.classId);
        this.setImage(await FFXIVApi.getIcon(classInfo.iconId));
    }
    
    private async _updateClassCache(): Promise<void> {
        let classes = await FFXIVApi.GameClass.getClasses(true);
        this.classCache = classes.sort((a, b) => (a.sortOrder > b.sortOrder) ? 1 : -1);
        
        for (const gameClass of this.classCache) {
            FFXIVApi.getIcon(gameClass.iconId).then((result) => {
                gameClass.iconData = result;
            });
        }
    }
    
    private async _renderDial(): Promise<void> {
        if (!this.classCache) return;
        let selectedClass = this.classCache[this.selectedClassIndex];
        
        if (selectedClass.iconData == null) {
            // just-in-time to be safe
            selectedClass.iconData = await FFXIVApi.getIcon(selectedClass.iconId);
        }
        
        this.setFeedback(<LayoutA1Feedback>{
            icon: selectedClass.iconData,
            value: selectedClass.abbreviation
        })
    }
}
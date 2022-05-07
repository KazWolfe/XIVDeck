import {BaseButton} from "../BaseButton";
import AbstractStateEvent from "@rweich/streamdeck-events/dist/Events/Received/Plugin/AbstractStateEvent";
import {KeyDownEvent} from "@rweich/streamdeck-events/dist/Events/Received/Plugin";
import plugin from "../../plugin";
import {FFXIVApi} from "../../link/ffxivplugin/FFXIVApi";

export type ClassButtonSettings = {
    classId: number,
    className: string
}

export class ClassButton extends BaseButton {
    classId: number;
    useGameIcon: boolean = true;

    constructor(event: AbstractStateEvent) {
        super(event.context);

        let settings = event.settings as ClassButtonSettings;
        this.classId = settings.classId;
        
        // try to render now, and schedule defer
        this.render();
        this._xivEventListeners.add(plugin.xivPluginLink.on("_ready", this.render.bind(this)));
    }
    
    async execute(event: KeyDownEvent): Promise<void> {
        if (this.classId == null) {
            throw new Error("No class specified for this button");
        }
        
        await FFXIVApi.GameClass.triggerClass(this.classId);
    }
    
    async render(): Promise<void> {
        if (!this.useGameIcon) {
            this.setImage("");
            return
        }

        if (!plugin.xivPluginLink.isReady()) {
            return;
        }

        if (this.classId == null) {
            return
        }
        
        let classInfo = await FFXIVApi.GameClass.getClass(this.classId);
        this.setImage(await FFXIVApi.getIcon(classInfo.iconId));
    }
}
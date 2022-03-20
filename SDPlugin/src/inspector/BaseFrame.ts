import piInstance from "../inspector";

export abstract class BaseFrame<T> {
    protected domParent = document.getElementById("dynamicPI")!;
    
    public abstract renderHTML(): void;
    
    public abstract loadSettings(settings: T): void;
    
    /* helpers */
    protected setSettings(settings: T): void {
        piInstance.sdPluginLink.setSettings(piInstance.uuid, settings);
    }
}
import {BaseFrame} from "../BaseFrame";
import {GlobalSettings} from "../../util/GlobalSettings";
import {PIUtils} from "../../util/PIUtils";
import piInstance from "../../inspector";

export class GlobalFrame extends BaseFrame<GlobalSettings> {
    domParent: HTMLElement;
    
    portField: HTMLInputElement;
    port: number = 37984;
    
    constructor() {
        super();
        this.domParent = document.getElementById("globalPI")!;
        this.portField = document.createElement("input");
        this.portField.type = "number";
        this.portField.min = "1024";
        this.portField.max = "59999";
        
        this.renderHTML();
    }
    
    renderHTML(): void {
        this.domParent.append(PIUtils.createPILabeledElement("XIVDeck Port", this.portField));
        
        this.portField.onchange = this._onPortChange.bind(this);
    }
    
    loadSettings(settings: GlobalSettings): void {
        this.port = settings.ws.port;
        
        this._loadElements();
    }
    
    private _loadElements() {
        this.portField.value = this.port.toString();
    }
    
    private _onPortChange() {
        if (!this.portField.validity.valid) {
            this.portField.setAttribute("style", "color: orangered");
            return;
        } else {
            this.portField.setAttribute("style", "");
        }
        
        this.port = parseInt(this.portField.value);
        
        piInstance.sdPluginLink.setGlobalSettings(piInstance.uuid, {
            "ws": {
                "port": this.port
            }
        })
    }
    
}
import {BaseFrame} from "../BaseFrame";
import {GlobalSettings} from "../../util/GlobalSettings";

export class GlobalFrame extends BaseFrame<GlobalSettings> {
    domParent: HTMLElement;
    
    constructor() {
        super();
        this.domParent = document.getElementById("globalPI")!;

        this.renderHTML();
    }
    
    renderHTML(): void {
        let htmlElement = document.createElement("p");
        htmlElement.innerText = "hello world";
        
        console.log(this);
        this.domParent.append(htmlElement);
    }
    
    loadSettings(settings: GlobalSettings): void {

    }
    
}
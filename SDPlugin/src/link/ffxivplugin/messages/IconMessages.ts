import {FFXIVOpcode} from "../MessageBase";

export class GetIconOpcode extends FFXIVOpcode {
    iconId: number;
    
    constructor(iconId: number) {
        super("getIcon");
        
        this.iconId = iconId;
    }
}


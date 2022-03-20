import {FFXIVOpcode} from "../MessageBase";

export class ExecuteHotbarSlotOpcode extends FFXIVOpcode {
    static override MESSAGE_OPCODE: string = "execHotbar";
    
    hotbarId: number;
    slotId: number;
    
    constructor(hotbarId: number, slotId: number) {
        super(ExecuteHotbarSlotOpcode.MESSAGE_OPCODE);
        
        this.hotbarId = hotbarId;
        this.slotId = slotId;
    }
}

export class GetHotbarSlotIconOpcode extends FFXIVOpcode {
    static override MESSAGE_OPCODE: string = "getHotbarIcon"

    hotbarId: number;
    slotId: number;

    constructor(hotbarId: number, slotId: number) {
        super(GetHotbarSlotIconOpcode.MESSAGE_OPCODE);

        this.hotbarId = hotbarId;
        this.slotId = slotId;
    }
}

export type HotbarSlotIconMessage = {
    hotbarId: number;
    slotId: number;
    
    iconId: number;
    iconData: string;
}
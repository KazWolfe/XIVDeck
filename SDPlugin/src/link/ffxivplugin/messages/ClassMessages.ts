import {FFXIVOpcode} from "../MessageBase";
import {FFXIVClass} from "../GameTypes";

export class GetClassesOpcode extends FFXIVOpcode {
    static override MESSAGE_OPCODE: string = "getClasses";
    
    constructor() {
        super(GetClassesOpcode.MESSAGE_OPCODE);
    }
}

export class GetClassOpcode extends FFXIVOpcode {
    static override MESSAGE_OPCODE = "getClass";
    
    id: number;

    constructor(classId: number) {
        super(GetClassOpcode.MESSAGE_OPCODE);
        this.id = classId;
    }
}

export class SwitchClassOpcode extends FFXIVOpcode {
    static override MESSAGE_OPCODE: string = "switchClass"
    
    id: number;

    constructor(classId: number) {
        super(SwitchClassOpcode.MESSAGE_OPCODE);
        this.id = classId;
    }
}

export type GameClassesMessage = {
    classes: FFXIVClass[];
    available: number[];
}

export type GameClassMessage = {
    class: FFXIVClass;
}
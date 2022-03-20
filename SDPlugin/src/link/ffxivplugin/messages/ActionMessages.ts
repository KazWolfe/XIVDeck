import {FFXIVOpcode} from "../MessageBase";
import {FFXIVAction} from "../GameTypes";

export class ExecuteActionOpcode extends FFXIVOpcode {
    static override MESSAGE_OPCODE: string = "execAction";
    
    action: FFXIVAction;
    
    constructor(actionType: string, actionId: number) {
        super(ExecuteActionOpcode.MESSAGE_OPCODE);
        this.action = { "id": actionId, "type": actionType } as FFXIVAction;
    }
}

export class GetActionIconOpcode extends FFXIVOpcode {
    static override MESSAGE_OPCODE: string = "getActionIcon"

    action: FFXIVAction

    constructor(actionType: string, actionId: number) {
        super(GetActionIconOpcode.MESSAGE_OPCODE);
        this.action = { "id": actionId, "type": actionType } as FFXIVAction;
    }
}

export class GetUnlockedActionsOpcode extends FFXIVOpcode {
    static override MESSAGE_OPCODE: string = "getUnlockedActions"

    constructor() {
        super(GetUnlockedActionsOpcode.MESSAGE_OPCODE);
    }  
}

export type ActionIconMessage = {
    action: FFXIVAction,
    iconId: number,
    iconData: string
}

export type UnlockedActionsMessage = {
    data: object
}
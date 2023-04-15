export abstract class FFXIVOpcode {
    static MESSAGE_OPCODE: string;
    
    opcode: string;

    constructor(opcode: string) {
        this.opcode = opcode;
    }
}

export abstract class FFXIVMessage {
    static MESSAGE_TYPE: string;
    
    messageType: string;
    
    constructor(messageType: string) {
        this.messageType = messageType;
    }
}
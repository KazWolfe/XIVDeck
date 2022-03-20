import { FFXIVOpcode } from "../../MessageBase";

export class InitOpcode extends FFXIVOpcode {
    static override MESSAGE_OPCODE = "init"
    
    version: string;
    
    constructor(version: string) {
        super(InitOpcode.MESSAGE_OPCODE);
        this.version = version;
    }
}
import {FFXIVOpcode} from "../MessageBase";

export class ExecuteCommandOpcode extends FFXIVOpcode {
    static override MESSAGE_OPCODE: string = "command";

    command: string;

    constructor(command: string) {
        super(ExecuteCommandOpcode.MESSAGE_OPCODE);
        this.command = command;
    }
}


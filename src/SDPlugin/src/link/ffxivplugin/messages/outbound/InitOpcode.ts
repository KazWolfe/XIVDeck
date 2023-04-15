import { FFXIVOpcode } from "../../MessageBase";

export type PluginMode = "Plugin" | "Inspector" | "Developer"

export class InitOpcode extends FFXIVOpcode {
    static override MESSAGE_OPCODE = "init"
    
    version: string;
    mode: PluginMode;
    
    constructor(version: string, mode: PluginMode) {
        super(InitOpcode.MESSAGE_OPCODE);
        this.version = version;
        this.mode = mode;
    }
}
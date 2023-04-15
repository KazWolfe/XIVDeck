import { FFXIVOpcode } from "../../MessageBase";

type OutgoingVolumePayload = {
    volume?: number,
    delta?: number,
    muted?: boolean
}

export class SetVolume extends FFXIVOpcode {
    static override MESSAGE_OPCODE = "setVolume"
    
    channel: string;
    data?: OutgoingVolumePayload;

    constructor(channel: string, payload: OutgoingVolumePayload) {
        super(SetVolume.MESSAGE_OPCODE);
        
        this.channel = channel;
        this.data = payload;
    }
}
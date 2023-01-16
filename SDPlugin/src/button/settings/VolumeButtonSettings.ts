export enum VolumeButtonMode {
    SET = "set",
    ADJUST = "adjust",
    MUTE = "mute"
}

export type VolumeButtonSettings = {
    channel: string,
    mode?: VolumeButtonMode,
    multiplier?: number,
    value?: number
}
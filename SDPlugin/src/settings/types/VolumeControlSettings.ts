import {JsonObject} from "@elgato/utils";

export enum VolumeControlMode {
    SET = "set",
    ADJUST = "adjust",
    MUTE = "mute",
}

export interface MuteVolumeControlSettings extends JsonObject {
    channel: string;
    mode?: VolumeControlMode.MUTE;
    multiplier?: number;
}

export interface SetVolumeControlSettings extends JsonObject {
    channel: string;
    mode: VolumeControlMode.SET;
    value: number;
}

export interface AdjustVolumeControlSettings extends JsonObject {
    channel: string;
    mode: VolumeControlMode.ADJUST;
    multiplier: number;
}

export type VolumeControlSettings = MuteVolumeControlSettings | SetVolumeControlSettings | AdjustVolumeControlSettings;

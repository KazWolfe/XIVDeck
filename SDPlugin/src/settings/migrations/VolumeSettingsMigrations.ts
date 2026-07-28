import {SettingsGateway} from "../SettingsGateway";
import {VolumeControlMode, VolumeControlSettings} from "../types/VolumeControlSettings";

SettingsGateway.register<VolumeControlSettings>("volume", {
    currentVersion: 0,
    steps: {},
    isComplete: (s): s is VolumeControlSettings => {
        if (typeof s.channel !== "string" || s.channel === "") return false;

        switch (s.mode ?? VolumeControlMode.MUTE) {
            case VolumeControlMode.MUTE:
                return true;
            case VolumeControlMode.SET:
                return typeof s.value === "number";
            case VolumeControlMode.ADJUST:
                return typeof s.multiplier === "number";
            default:
                return false;
        }
    },
});

import {SettingsGateway} from "../SettingsGateway";
import {ClassButtonSettings} from "../types/ClassButtonSettings";

SettingsGateway.register<ClassButtonSettings>("class", {
    currentVersion: 0,
    steps: {},
    isComplete: (s): s is ClassButtonSettings => typeof s.classId === "number",
});

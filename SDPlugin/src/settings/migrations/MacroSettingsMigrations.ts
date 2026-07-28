import {SettingsGateway} from "../SettingsGateway";
import {MacroButtonSettings} from "../types/MacroButtonSettings";

SettingsGateway.register<MacroButtonSettings>("macro", {
    currentVersion: 0,
    steps: {},
    isComplete: (s): s is MacroButtonSettings => typeof s.macroId === "number",
});

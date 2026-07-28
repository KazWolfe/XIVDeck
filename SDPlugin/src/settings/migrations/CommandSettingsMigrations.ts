import {SettingsGateway} from "../SettingsGateway";
import {CommandButtonSettings} from "../types/CommandButtonSettings";

SettingsGateway.register<CommandButtonSettings>("command", {
    currentVersion: 0,
    steps: {},
    // A bare "/" is what the property inspector shows for an empty command.
    isComplete: (s): s is CommandButtonSettings => typeof s.command === "string" && s.command !== "" && s.command !== "/",
});

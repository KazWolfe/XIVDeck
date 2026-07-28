import {SettingsGateway} from "../SettingsGateway";
import {HotbarButtonSettings} from "../types/HotbarButtonSettings";

interface HotbarButtonSettingsV0 {
    // -1 meant "unset".
    hotbarId?: number;
    slotId?: number;
}

SettingsGateway.register<HotbarButtonSettings>("hotbar", {
    currentVersion: 1,
    steps: {
        0: (v0: HotbarButtonSettingsV0): Partial<HotbarButtonSettings> => ({
            _v: 1,
            hotbarId: v0.hotbarId != null && v0.hotbarId >= 0 ? v0.hotbarId : undefined,
            slotId: v0.slotId != null && v0.slotId >= 0 ? v0.slotId : undefined,
        }),
    },
    isComplete: (s): s is HotbarButtonSettings => typeof s.hotbarId === "number" && typeof s.slotId === "number",
});

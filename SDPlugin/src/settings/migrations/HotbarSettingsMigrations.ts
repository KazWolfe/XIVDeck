import {MigrationChain} from "#/settings/SettingsMigrator";
import {HotbarButtonSettings} from "#/settings/types/HotbarButtonSettings";

interface HotbarButtonSettingsV0 {
    hotbarId: number;
    slotId: number;
}

export const HotbarSettingsMigrations: MigrationChain<HotbarButtonSettings> = {
    currentVersion: 1,
    steps: {
        0: (v0: HotbarButtonSettingsV0): HotbarButtonSettings => ({
            _v: 1,
            hotbarId: v0.hotbarId,
            slotId: v0.slotId,
        }),
    },
};

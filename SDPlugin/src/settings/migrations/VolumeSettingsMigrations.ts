import {MigrationChain} from "#/settings/SettingsMigrator";
import {VolumeControlSettings} from "#/settings/types/VolumeControlSettings";

export const VolumeSettingsMigrations: MigrationChain<VolumeControlSettings> = {
    currentVersion: 0,
    steps: {},
};

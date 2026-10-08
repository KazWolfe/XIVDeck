import {MigrationChain} from "#/settings/SettingsMigrator";
import {ExecActionSettings} from "#/settings/types/ExecActionSettings";
import {ActionEntry} from "#/client/rpc/messages/Action";

// we've moved over to using the EXD names for everything, so convert things over.
const LEGACY_ACTION_TYPES: Record<string, string> = {
    Collection: "McGuffin",
    Minion: "Companion",
    FashionAccessory: "Ornament",
};

interface ExecActionSettingsV0 {
    actionType: string;
    actionId: number;
    actionName?: string;

    cache?: ActionEntry;
    payload?: unknown;
}

export const ExecActionSettingsMigrations: MigrationChain<ExecActionSettings> = {
    currentVersion: 1,
    steps: {
        0: (v0: ExecActionSettingsV0): ExecActionSettings => ({
            _v: 1,
            actionType: LEGACY_ACTION_TYPES[v0.actionType] ?? v0.actionType,
            actionId: v0.actionId,
            payload: v0.payload as ExecActionSettings["payload"],
            cache: v0.cache,
        }),
    },
};

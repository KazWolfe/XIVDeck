import {JsonObject} from "@elgato/utils";

export type MigrationStep = (from: never) => unknown;

export interface MigrationChain<TCurrent> {
    steps: Record<number, MigrationStep>;
    currentVersion: number;
}

/**
 * Simple migration/config helper. Bring a stored setting up to the current version, based on the defined chain.
 *
 * Note that we intentionally take in `JsonObject` rather than a concrete type, since we don't necessarily have a
 * guarantee of typing. It's on the migration to handle this, unfortunately.
 */
export class SettingsMigrator {
    /** Runs every step from the stored `_v` (default 0) up to `chain.currentVersion`. Returns `raw` itself if nothing ran. */
    public static migrate<TCurrent>(chain: MigrationChain<TCurrent>, raw: JsonObject): TCurrent {
        let version = typeof raw._v === "number" ? raw._v : 0;
        let value = raw;

        while (version < chain.currentVersion) {
            const step = chain.steps[version];
            if (!step) {
                throw new Error(`No migration step from settings version ${version}.`);
            }

            value = (step as (from: JsonObject) => JsonObject)(value);
            version++;
        }

        // Each step produces the next complete version, so after the last one this is the current shape.
        return value as TCurrent;
    }

    /**
     * Utility function to check if a settings object is empty.
     */
    public static isEmpty(settings: JsonObject): boolean {
        return Object.keys(settings).length === 0;
    }

    public static orEmpty<T extends JsonObject>(value: T): T | undefined {
        return this.isEmpty(value) ? undefined : value;
    }
}

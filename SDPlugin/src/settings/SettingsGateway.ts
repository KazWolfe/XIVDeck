export type MigrationStep<TTo = unknown> = (from: any) => TTo;

export interface MigrationChain<TCurrent> {
    // keyed on originating version, to next version.
    steps: Record<number, MigrationStep>;
    currentVersion: number;

    // The property inspector saves one field at a time, so stored settings are routinely partial.
    isComplete?: (settings: any) => settings is TCurrent;
}

export class SettingsGateway {
    private static _chains = new Map<string, MigrationChain<unknown>>();

    public static register<TCurrent>(kind: string, chain: MigrationChain<TCurrent>): void {
        this._chains.set(kind, chain);
    }

    public static load<TStored>(kind: string, raw: unknown): TStored {
        const chain = this._getChain(kind);

        let version = (raw as { _v?: number } | undefined)?._v ?? 0;
        let value: any = raw ?? {};

        while (version < chain.currentVersion) {
            const step = chain.steps[version];
            if (!step) {
                throw new Error(`No migration step registered for "${kind}" from version ${version}.`);
            }

            value = step(value);
            version++;
        }

        return value as TStored;
    }

    public static complete<TCurrent>(kind: string, migrated: unknown): TCurrent | undefined {
        const {isComplete} = this._getChain(kind);
        if (!isComplete) return migrated as TCurrent;

        return isComplete(migrated) ? migrated as TCurrent : undefined;
    }

    private static _getChain(kind: string): MigrationChain<unknown> {
        const chain = this._chains.get(kind);
        if (!chain) {
            throw new Error(`No migration chain registered for settings kind "${kind}".`);
        }

        return chain;
    }
}

import {DefaultGlobalSettings, GlobalSettings} from "./GlobalSettings";

export interface GlobalSettingsProvider {
    getSettings(): GlobalSettings;
}

export class GlobalSettingsStore implements GlobalSettingsProvider {
    private _settings: GlobalSettings = DefaultGlobalSettings;

    getSettings(): GlobalSettings {
        return this._settings;
    }

    update(settings: GlobalSettings): void {
        this._settings = settings;
    }
}

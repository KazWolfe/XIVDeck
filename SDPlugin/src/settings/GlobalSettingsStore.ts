import {injectable} from "inversify";
import {DefaultGlobalSettings, GlobalSettings} from "./GlobalSettings";
import {EventSource, IEvent} from "#/util/EventSource";

@injectable()
export class GlobalSettingsStore {
    private readonly _changed = new EventSource<[current: GlobalSettings, previous: GlobalSettings]>();
    private _current: GlobalSettings = DefaultGlobalSettings;

    public readonly changed: IEvent<[current: GlobalSettings, previous: GlobalSettings]> = this._changed;

    public get current(): GlobalSettings {
        return this._current;
    }

    public get trackCooldowns(): boolean {
        return this._current.trackCooldowns ?? true;
    }

    public update(settings: GlobalSettings): void {
        const previous = this._current;
        this._current = settings;
        this._changed.emit(settings, previous);
    }
}

import {EventHandler, EventSource} from "./EventSource";

type EventMap<T> = { [K in keyof T]: unknown[] };

export class KeyedEventEmitter<TEvents extends EventMap<TEvents>> {
    private readonly _sources = new Map<keyof TEvents, EventSource<unknown[]>>();

    public subscribe<K extends keyof TEvents>(name: K, handler: EventHandler<TEvents[K]>): void {
        let source = this._sources.get(name);
        if (!source) {
            source = new EventSource<unknown[]>();
            this._sources.set(name, source);
        }

        source.add(handler as EventHandler<unknown[]>);
    }

    public unsubscribe<K extends keyof TEvents>(name: K, handler: EventHandler<TEvents[K]>): void {
        this._sources.get(name)?.remove(handler as EventHandler<unknown[]>);
    }

    public emit<K extends keyof TEvents>(name: K, ...args: TEvents[K]): void {
        this._sources.get(name)?.emit(...args);
    }

    public clear(): void {
        this._sources.clear();
    }
}

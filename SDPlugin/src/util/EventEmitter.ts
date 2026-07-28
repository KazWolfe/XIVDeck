type Listener<TArgs extends unknown[]> = (...args: TArgs) => void;

type ArgsOf<TEvents extends object, K extends keyof TEvents> = TEvents[K] extends unknown[] ? TEvents[K] : never;

export type ListenerErrorHandler = (event: PropertyKey, err: unknown) => void;

export class EventEmitter<TEvents extends object = Record<string, unknown[]>> {
    static onListenerError: ListenerErrorHandler = (event, err) => {
        console.error(`EventEmitter: listener for "${String(event)}" failed:`, err);
    };

    private _eventList = new Map<keyof TEvents, PubSub<any>>();

    on<K extends keyof TEvents>(name: K, fn: Listener<ArgsOf<TEvents, K>>): () => void {
        if (!this._eventList.has(name)) {
            this._eventList.set(name, new PubSub());
        }
        return this._eventList.get(name)!.sub(fn);
    };

    emit<K extends keyof TEvents>(name: K, ...args: ArgsOf<TEvents, K>) {
        this._eventList.get(name)?.pub(err => EventEmitter._reportListenerError(name, err), ...args);
    }

    private static _reportListenerError(event: PropertyKey, err: unknown): void {
        try {
            EventEmitter.onListenerError(event, err);
        } catch {
            // meh.
        }
    }
}

class PubSub<TArgs extends unknown[]> {
    private subscribers = new Set<Listener<TArgs>>();

    sub(fn: Listener<TArgs>): () => void {
        this.subscribers.add(fn);

        return () => {
            this.subscribers.delete(fn);
        }
    }

    pub(onError: (err: unknown) => void, ...args: TArgs) {
        for (const fn of [...this.subscribers]) {
            try {
                const result: unknown = fn(...args);
                if (result instanceof Promise) {
                    result.catch(onError);
                }
            } catch (err) {
                onError(err);
            }
        }
    }
}

export type EventHandler<TArgs extends unknown[]> = (...args: TArgs) => void | Promise<void>;

export interface IEvent<TArgs extends unknown[] = []> {
    add(handler: EventHandler<TArgs>): void;

    remove(handler: EventHandler<TArgs>): void;
}

export class EventSource<TArgs extends unknown[] = []> implements IEvent<TArgs> {
    private readonly _handlers = new Set<EventHandler<TArgs>>();

    public add(handler: EventHandler<TArgs>): void {
        this._handlers.add(handler);
    }

    public remove(handler: EventHandler<TArgs>): void {
        this._handlers.delete(handler);
    }

    public emit(...args: TArgs): void {
        // snapshot, so handlers may add/remove during dispatch.
        for (const handler of [...this._handlers]) {
            try {
                const result = handler(...args);
                if (result instanceof Promise) {
                    result.catch(EventSource.reportUnhandled);
                }
            } catch (err) {
                EventSource.reportUnhandled(err);
            }
        }
    }

    /** Drops every handler. Owners call this on dispose so nothing outlives them. */
    public clear(): void {
        this._handlers.clear();
    }

    private static reportUnhandled(err: unknown): void {
        console.error("[EventSource] Unhandled error in event handler:", err);
    }
}

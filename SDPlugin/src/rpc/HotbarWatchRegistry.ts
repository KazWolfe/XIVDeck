import streamDeck from "@elgato/streamdeck";
import {ClientProvider} from "./ClientProvider";
import {XivDeckClient} from "./XivDeckClient";
import {HotbarChangeNotification, HotbarSlotRef} from "./messages/Hotbar";
import {DEFAULT_TRANSPORT_ID} from "../settings/GlobalSettings";
import {Debouncer} from "../util/Debouncer";

const FLUSH_DELAY_MS = 50;

interface Watcher {
    key: string;
    onChanged: () => void;
}

function toKey(slot: HotbarSlotRef): string {
    return `${slot.hotbarId}:${slot.slotId}`;
}

function fromKey(key: string): HotbarSlotRef {
    const [hotbarId, slotId] = key.split(":").map(Number);
    return {hotbarId, slotId};
}

/**
 * Assistant service to indicate to the game plugin what hotbar slots need to be watched.
 * Updates on every add/remove/config change, waits for a flush, and then fires.
 */
export class HotbarWatchRegistry {
    private readonly _watchers = new Map<string, Watcher>();
    private readonly _flush = new Debouncer(FLUSH_DELAY_MS, () => void this._send());

    private _sent = new Set<string>();
    private _sendGeneration = 0;
    private _unsubscribers: (() => void)[] = [];

    constructor(private readonly clients: ClientProvider, private readonly transportId = DEFAULT_TRANSPORT_ID) {
        this._attach();

        clients.onClientChanged(transportId => {
            if (transportId !== this.transportId) return;
            this._attach();
        });
    }

    watch(context: string, slot: HotbarSlotRef, onChanged: () => void): void {
        const key = toKey(slot);
        if (this._watchers.get(context)?.key === key) return;

        this._watchers.set(context, {key, onChanged});
        this._flush.schedule();
    }

    release(context: string): void {
        if (this._watchers.delete(context)) this._flush.schedule();
    }

    private get _client(): XivDeckClient {
        return this.clients.getClient(this.transportId);
    }

    private _attach(): void {
        for (const unsubscribe of this._unsubscribers) unsubscribe();

        const client = this._client;
        this._unsubscribers = [
            client.on("Hotbar.OnHotbarChanged", notification => this._onHotbarChanged(notification)),
            client.on("_ready", () => {
                this._sent = new Set();
                this._flush.cancel();
                void this._send();
            }),
            client.on("_closed", () => this._sent = new Set()),
        ];

        this._sent = new Set();
        if (client.isReady()) this._flush.schedule();
    }

    private async _send(): Promise<void> {
        const client = this._client;
        if (!client.isReady()) return;

        const wanted = new Set([...this._watchers.values()].map(w => w.key));
        if (wanted.size === this._sent.size && [...wanted].every(key => this._sent.has(key))) return;

        const added = [...wanted].filter(key => !this._sent.has(key));
        const generation = ++this._sendGeneration;

        try {
            await client.request("Hotbar.SetWatchedSlots", {watchRequest: {slots: [...wanted].map(fromKey)}});
        } catch (err) {
            streamDeck.logger.warn("Failed to update watched hotbar slots:", err);
            return;
        }

        if (generation !== this._sendGeneration) return;
        this._sent = wanted;

        const addedKeys = new Set(added);
        for (const watcher of this._watchers.values()) {
            if (addedKeys.has(watcher.key)) watcher.onChanged();
        }
    }

    private _onHotbarChanged(notification: HotbarChangeNotification): void {
        const changed = notification.changes && new Set(notification.changes.map(toKey));

        for (const watcher of this._watchers.values()) {
            if (!changed || changed.has(watcher.key)) watcher.onChanged();
        }
    }
}

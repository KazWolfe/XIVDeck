import streamDeck from "@elgato/streamdeck";
import {injectable} from "inversify";
import {GameConnection} from "../rpc/GameConnection";
import {HotbarChangeNotification, HotbarSlotRef} from "#/client/rpc/messages/Hotbar";

export interface IHotbarSubscriber {
    onHotbarSlotChanged(): void;
}

const FLUSH_DELAY_MS = 50;

/**
 * Maintains the game's watched-slot set for one client. Each owner subscribes to at most one slot; the union of all
 * subscriptions is sent via `Hotbar.SetWatchedSlots` (debounced), and re-sent whenever the connection becomes ready.
 * Client-scoped.
 */
@injectable()
export class HotbarTracker implements Disposable {
    private readonly _slots = new Map<IHotbarSubscriber, HotbarSlotRef>();

    /** Keys of the slots the game last confirmed it is watching. */
    private _sent = new Set<string>();

    /** The set most recently sent; a reply for any older set is stale. */
    private _inFlight: Map<string, HotbarSlotRef> | undefined;

    private _flushTimer: ReturnType<typeof setTimeout> | undefined;

    public constructor(private readonly connection: GameConnection) {
        this.connection.ready.add(this.send);
        this.connection.closed.add(this.onClosed);
        this.connection.subscribe("Hotbar.OnHotbarChanged", this.onHotbarChanged);
    }

    /** Subscribes `owner` to `slot`, replacing any slot it was already subscribed to. */
    public subscribe(owner: IHotbarSubscriber, slot: HotbarSlotRef): void {
        const current = this._slots.get(owner);
        if (current && HotbarTracker.toKey(current) === HotbarTracker.toKey(slot)) return;

        this._slots.set(owner, slot);
        this.scheduleSend();
    }

    public unsubscribe(owner: IHotbarSubscriber): void {
        if (this._slots.delete(owner)) this.scheduleSend();
    }

    public [Symbol.dispose](): void {
        this.connection.ready.remove(this.send);
        this.connection.closed.remove(this.onClosed);
        this.connection.unsubscribe("Hotbar.OnHotbarChanged", this.onHotbarChanged);

        clearTimeout(this._flushTimer);
        this._slots.clear();
    }

    private readonly onClosed = (): void => {
        this._sent = new Set();
        this._inFlight = undefined;
    };

    private readonly onHotbarChanged = (notification: HotbarChangeNotification): void => {
        const changed = notification.changes && new Set(notification.changes.map(HotbarTracker.toKey));

        for (const [owner, slot] of [...this._slots]) {
            if (!changed || changed.has(HotbarTracker.toKey(slot))) owner.onHotbarSlotChanged();
        }
    };

    private scheduleSend(): void {
        clearTimeout(this._flushTimer);
        this._flushTimer = setTimeout(this.send, FLUSH_DELAY_MS);
    }

    /** Sends the current union of subscriptions, if it differs from what the game last confirmed. */
    private readonly send = async (): Promise<void> => {
        if (!this.connection.isReady) return;

        const wanted = new Map([...this._slots.values()].map(slot => [HotbarTracker.toKey(slot), slot]));
        const added = new Set([...wanted.keys()].filter(key => !this._sent.has(key)));
        if (added.size === 0 && wanted.size === this._sent.size) return;

        this._inFlight = wanted;

        try {
            await this.connection.request("Hotbar.SetWatchedSlots", {
                watchRequest: {slots: [...wanted.values()]},
            });
        } catch (err) {
            streamDeck.logger.warn("Failed to update watched hotbar slots:", err);
            return;
        }

        if (this._inFlight !== wanted) return;
        this._sent = new Set(wanted.keys());

        for (const [owner, slot] of [...this._slots]) {
            if (added.has(HotbarTracker.toKey(slot))) owner.onHotbarSlotChanged();
        }
    };

    private static toKey(slot: HotbarSlotRef): string {
        return `${slot.hotbarId}:${slot.slotId}`;
    }
}

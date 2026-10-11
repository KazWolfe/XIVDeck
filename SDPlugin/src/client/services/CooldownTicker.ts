import {injectable} from "inversify";
import {GameConnection} from "../rpc/GameConnection";
import {CooldownGroupDetail, CooldownNotification} from "#/client/rpc/messages/ActionAppearance";
import {GlobalSettingsStore} from "#/settings/GlobalSettingsStore";

export interface ICooldownSubscriber {
    /**
     * Indicates a `Cooldown.CooldownStart` message from the game, noting an action has been used and a cooldown
     * has begun ticking.
     */
    onCooldownStarted(): void;

    /**
     * Called whenever time advances while a watched group is running, and once more after it runs out.
     */
    onCooldownTick(): void;
}

export interface CooldownTiming {
    /** Unix ms. */
    startTime: number;
    /** Unix ms. */
    endTime: number;
}

const COOLDOWN_TICK_MS = 200;

/**
 * Cooldown state and a shared timer for tracking and rendering, on a per-client basis. The timer only runs while
 * tracking is enabled and some watched group is active.
 *
 * This is the only source of cooldown timing for rendering. Snapshots only seed it, so the timer and every control
 * watching a group always agree on when that group ends.
 */
@injectable()
export class CooldownTicker implements Disposable {
    private readonly _timings = new Map<number, CooldownTiming>();
    private readonly _watches = new Map<ICooldownSubscriber, readonly number[]>();

    private _timer: ReturnType<typeof setInterval> | undefined;
    private _lastTick = 0;

    public constructor(private readonly connection: GameConnection, private readonly settings: GlobalSettingsStore) {
        this.connection.subscribe("Cooldown.CooldownStart", this.onCooldownStart);
        this.settings.changed.add(this.syncTimer);
    }

    /**
     * Load the specified cooldown groups into the tracker. Used for initial seeding on render calls, et al.
     * Most updates will come through `Cooldown.CooldownStart`, but this allows us to start tracking the moment
     * a control comes online.
     */
    public setTimersFromGroups(groups: readonly CooldownGroupDetail[]): void {
        for (const group of groups) {
            this._timings.set(group.groupId, {startTime: group.startTime, endTime: group.endTime});
        }

        this.syncTimer();
    }

    public timingOf(groupId: number): CooldownTiming | undefined {
        return this._timings.get(groupId);
    }

    /** Replaces the set of groups `owner` watches. An empty list is equivalent to {@link unsubscribe}. */
    public subscribe(owner: ICooldownSubscriber, groupIds: readonly number[]): void {
        if (groupIds.length === 0) {
            this.unsubscribe(owner);
            return;
        }

        this._watches.set(owner, groupIds);
        this.syncTimer();
    }

    public unsubscribe(owner: ICooldownSubscriber): void {
        if (this._watches.delete(owner)) this.syncTimer();
    }

    public [Symbol.dispose](): void {
        this.connection.unsubscribe("Cooldown.CooldownStart", this.onCooldownStart);
        this.settings.changed.remove(this.syncTimer);

        clearInterval(this._timer);
        this._timer = undefined;
        this._watches.clear();
        this._timings.clear();
    }

    private readonly onCooldownStart = (notification: CooldownNotification): void => {
        // Recorded even while tracking is off, so turning it back on shows the right state.
        this._timings.set(notification.groupId, {startTime: notification.startTime, endTime: notification.endTime});

        if (!this.settings.trackCooldowns) return;

        for (const [owner, groupIds] of [...this._watches]) {
            if (groupIds.includes(notification.groupId)) owner.onCooldownStarted();
        }

        this.syncTimer();
    };

    private readonly onTick = (): void => {
        const since = this._lastTick;
        const now = Date.now();
        this._lastTick = now;

        // a group that ran out since the last tick gets one more, so its finished state is drawn.
        for (const [owner, groupIds] of [...this._watches]) {
            if (this.latestEnd(groupIds) > since) owner.onCooldownTick();
        }

        // same `now` as the ticks above: a group ending between the two would otherwise never get its final tick.
        this.sync(now);
    };

    private readonly syncTimer = (): void => {
        this.sync(Date.now());
    };

    private sync(now: number): void {
        const running = this.settings.trackCooldowns
            && [...this._watches.values()].some(groupIds => this.latestEnd(groupIds) > now);

        if (!running) {
            clearInterval(this._timer);
            this._timer = undefined;
        } else if (this._timer === undefined) {
            this._lastTick = now;
            this._timer = setInterval(this.onTick, COOLDOWN_TICK_MS);
        }
    }

    private latestEnd(groupIds: readonly number[]): number {
        return Math.max(0, ...groupIds.map(groupId => this._timings.get(groupId)?.endTime ?? 0));
    }
}

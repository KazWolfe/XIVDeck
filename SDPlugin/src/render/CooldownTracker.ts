import {ActionAppearance, CooldownNotification} from "../rpc/messages/ActionAppearance";
import {isCooldownActive} from "./CooldownMath";
import {IntervalTicker} from "../util/IntervalTicker";
import {GlobalSettingsProvider} from "../settings/GlobalSettingsProvider";

const COOLDOWN_TICK_MS = 200;

export class CooldownTracker {
    private readonly _trackedGroupIds = new Set<number>();
    private readonly _ticker: IntervalTicker;

    constructor(onTick: () => void, private readonly globalSettings: GlobalSettingsProvider) {
        this._ticker = new IntervalTicker(COOLDOWN_TICK_MS, onTick);
    }

    get trackingEnabled(): boolean {
        return this.globalSettings.getSettings().trackCooldowns ?? true;
    }

    sync(appearance: ActionAppearance): void {
        this._trackedGroupIds.clear();

        if (!this.trackingEnabled) {
            this._ticker.sync(false);
            return;
        }

        const cooldown = appearance.cooldownDetails;
        if (cooldown?.recastGroup) {
            this._trackedGroupIds.add(cooldown.recastGroup.groupId);
        }
        if (cooldown?.rechargeGroup) {
            this._trackedGroupIds.add(cooldown.rechargeGroup.groupId);
        }

        this._ticker.sync(this.isActive(appearance));
    }

    isTracked(detail: CooldownNotification): boolean {
        if (!this.trackingEnabled) return false;

        return this._trackedGroupIds.has(detail.groupId);
    }

    isActive(appearance: ActionAppearance, now = Date.now()): boolean {
        return isCooldownActive(appearance, now, this.trackingEnabled);
    }

    stop(): void {
        this._ticker.stop();
    }
}

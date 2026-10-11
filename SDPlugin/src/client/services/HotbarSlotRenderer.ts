import {injectable} from "inversify";
import {
    ActionAppearance,
    ActionCooldownDetail,
    ActionCostType,
    CooldownGroupDetail,
    CooldownRenderMode,
} from "#/client/rpc/messages/ActionAppearance";
import {GlobalSettingsStore} from "#/settings/GlobalSettingsStore";
import {CooldownTicker} from "./CooldownTicker";
import {HotbarSlotDocument} from "./HotbarSlotDocument";

export interface CooldownOverlay {
    recastElapsedPercent?: number;
    chargeCount?: number;
    rechargeDonutPercent?: number;
    rechargeSweepPercent?: number;

    // Optional field that overrides the cost object.
    secondsRemaining?: number;
}

interface IChargeInterval {
    chargeIntervalMs: number;
    intoCurrentCharge: number;
}

@injectable()
export class HotbarSlotRenderer {
    public constructor(private readonly settings: GlobalSettingsStore, private readonly cooldowns: CooldownTicker) {
    }

    public buildSlot(appearance: ActionAppearance, iconBase64: string, typeIconSvg: string | undefined): string {
        const slot = new HotbarSlotDocument();

        slot.setIcon(iconBase64);
        slot.setTypeIcon(typeIconSvg);

        const details = this.settings.trackCooldowns ? appearance.cooldownDetails : null;
        const overlay = details ? this.buildCooldownOverlay(details, Date.now()) : undefined;

        if (overlay) {
            slot.setRecastTimer(overlay.recastElapsedPercent);
            slot.setChargeCount(overlay.chargeCount);
            slot.setRechargeTimerDonut(overlay.rechargeDonutPercent);
            slot.setRechargeTimerSweep(overlay.rechargeSweepPercent);
        }

        // game quirk: a running cooldown's timer replaces the action's own cost label.
        if (overlay?.secondsRemaining != null) {
            slot.setCost({text: String(overlay.secondsRemaining), type: ActionCostType.Default, position: "left"});
        } else if (appearance.costText) {
            const position = appearance.costRightJustified ? "right" : "left";
            slot.setCost({text: appearance.costText, type: appearance.costType, position});
        }

        return slot.toDataUrl();
    }

    private buildCooldownOverlay(details: ActionCooldownDetail, now: number): CooldownOverlay {
        const overlay: CooldownOverlay = {};
        const rechargeGroup = this.resolveGroup(details.rechargeGroup, now);
        const recastGroup = this.resolveGroup(details.recastGroup, now);

        if (rechargeGroup) {
            HotbarSlotRenderer.addGroupToOverlay(overlay, rechargeGroup, now);
        }
        if (recastGroup) {
            HotbarSlotRenderer.addGroupToOverlay(overlay, recastGroup, now);
        }

        overlay.secondsRemaining = HotbarSlotRenderer.getCooldownSeconds(rechargeGroup, now)
            ?? HotbarSlotRenderer.getCooldownSeconds(recastGroup, now);

        return overlay;
    }

    /**
     * Applies the ticker's timing to a snapshot group, falling back to the snapshot's own if the ticker hasn't seen
     * the group yet. Charges are derived from that timing, as the snapshot's count goes stale as soon as one comes back.
     */
    private resolveGroup(group: CooldownGroupDetail | null | undefined, now: number): CooldownGroupDetail | undefined {
        if (group == null) return undefined;

        const timing = this.cooldowns.timingOf(group.groupId) ?? group;
        const resolved = {...group, startTime: timing.startTime, endTime: timing.endTime};

        if (now >= resolved.endTime) {
            resolved.currentCharges = resolved.maxCharges;
        } else {
            const interval = HotbarSlotRenderer.chargeInterval(resolved, now);
            if (interval) {
                const charges = Math.floor((now - resolved.startTime) / interval.chargeIntervalMs);
                resolved.currentCharges = Math.min(resolved.maxCharges, Math.max(0, charges));
            }
        }

        return resolved;
    }

    private static getCooldownSeconds(group: CooldownGroupDetail | null | undefined, now: number): number | undefined {
        if (!HotbarSlotRenderer.isGroupActive(group, now) || group.hideTimerLabel) return undefined;

        if (group.renderMode === CooldownRenderMode.RecastSweep) {
            return Math.max(0, Math.ceil((group.endTime - now) / 1000));
        }

        const interval = HotbarSlotRenderer.chargeInterval(group, now);
        if (!interval) return undefined;

        return Math.max(0, Math.ceil((interval.chargeIntervalMs - interval.intoCurrentCharge) / 1000));
    }

    private static addGroupToOverlay(overlay: CooldownOverlay, group: CooldownGroupDetail, now: number): void {
        switch (group.renderMode) {
            case CooldownRenderMode.None:
                return;

            case CooldownRenderMode.RecastSweep:
                overlay.recastElapsedPercent = HotbarSlotRenderer.elapsedPercent(group, now);
                return;

            case CooldownRenderMode.ChargeRing:
                if (group.maxCharges > 1) {
                    overlay.chargeCount = group.currentCharges;
                }
                if (group.currentCharges < group.maxCharges) {
                    overlay.rechargeDonutPercent = HotbarSlotRenderer.chargeFillPercent(group, now);
                }
                return;

            case CooldownRenderMode.ChargeSweep:
                if (group.maxCharges > 1) {
                    overlay.chargeCount = group.currentCharges;
                }
                if (group.currentCharges < group.maxCharges) {
                    overlay.rechargeSweepPercent = HotbarSlotRenderer.chargeFillPercent(group, now);
                }
                return;
        }
    }

    private static isGroupActive(
        group: CooldownGroupDetail | null | undefined, now: number,
    ): group is CooldownGroupDetail {
        return group != null && group.renderMode !== CooldownRenderMode.None && now < group.endTime;
    }

    private static elapsedPercent(group: CooldownGroupDetail, now: number): number {
        const duration = group.endTime - group.startTime;
        if (duration <= 0) return 100;

        return Math.min(100, Math.max(0, ((now - group.startTime) / duration) * 100));
    }

    private static chargeFillPercent(group: CooldownGroupDetail, now: number): number {
        const interval = HotbarSlotRenderer.chargeInterval(group, now);
        if (!interval) return 100;

        return Math.min(100, Math.max(0, (interval.intoCurrentCharge / interval.chargeIntervalMs) * 100));
    }

    private static chargeInterval(group: CooldownGroupDetail, now: number): IChargeInterval | undefined {
        const totalDuration = group.endTime - group.startTime;
        if (totalDuration <= 0 || group.maxCharges <= 0) return undefined;

        const chargeIntervalMs = totalDuration / group.maxCharges;

        return {chargeIntervalMs, intoCurrentCharge: (now - group.startTime) % chargeIntervalMs};
    }
}

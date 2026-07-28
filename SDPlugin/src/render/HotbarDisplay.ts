import {ActionAppearance, ActionCostType, CooldownGroupDetail, CooldownRenderMode} from "../rpc/messages/ActionAppearance";
import {hotbarSlotRenderer, HotbarSlotRenderOptions} from "./HotbarSlotRenderer";
import {isGroupActive} from "./CooldownMath";
import {CooldownTracker} from "./CooldownTracker";

export interface ResolvedActionIcons {
    iconBase64: string;
    typeIconBase64?: string;
    typeIconSvg?: string;
}

export class HotbarDisplay {
    constructor(private readonly cooldownTracker: CooldownTracker) {
    }

    render(appearance: ActionAppearance, icons: ResolvedActionIcons, now = Date.now()): string {
        return hotbarSlotRenderer.render(this.buildRenderOptions(appearance, icons, now));
    }

    private buildRenderOptions(appearance: ActionAppearance, icons: ResolvedActionIcons, now: number): HotbarSlotRenderOptions {
        const cooldown = this.cooldownTracker.trackingEnabled ? appearance.cooldownDetails : undefined;

        const options: HotbarSlotRenderOptions = {
            iconBase64: icons.iconBase64,
            typeIcon: buildTypeIconMarkup(icons),
            cost: appearance.costText ? {
                text: appearance.costText,
                type: appearance.costType,
                position: appearance.costRightJustified ? "right" : "left",
            } : undefined,
        };

        const recastGroup = cooldown?.recastGroup;
        const rechargeGroup = cooldown?.rechargeGroup;

        if (rechargeGroup) {
            applyGroupRender(options, rechargeGroup, now);
        }
        if (recastGroup) {
            applyGroupRender(options, recastGroup, now);
        }

        const cooldownSecondsRemaining = groupSecondsRemaining(rechargeGroup, now) ?? groupSecondsRemaining(recastGroup, now);

        if (cooldownSecondsRemaining != null) {
            options.cost = {
                text: String(cooldownSecondsRemaining),
                type: ActionCostType.Default,
                position: "left",
            };
        }

        return options;
    }
}

function buildTypeIconMarkup(icons: ResolvedActionIcons): string | undefined {
    if (icons.typeIconSvg) return icons.typeIconSvg;
    if (icons.typeIconBase64) {
        return `<image x="0" y="0" width="18" height="18" xlink:href="data:image/png;base64,${icons.typeIconBase64}"/>`;
    }

    return undefined;
}

function groupSecondsRemaining(group: CooldownGroupDetail | null | undefined, now: number): number | undefined {
    if (!isGroupActive(group, now) || group!.hideTimerLabel) return undefined;

    if (group!.renderMode === CooldownRenderMode.RecastSweep) {
        return Math.max(0, Math.ceil((group!.endTime - now) / 1000));
    }

    const interval = chargeInterval(group!, now);
    if (!interval) return undefined;

    return Math.max(0, Math.ceil((interval.chargeIntervalMs - interval.intoCurrentCharge) / 1000));
}

function applyGroupRender(options: HotbarSlotRenderOptions, group: CooldownGroupDetail, now: number): void {
    switch (group.renderMode) {
        case CooldownRenderMode.None:
            return;

        case CooldownRenderMode.RecastSweep:
            options.cooldownElapsedPercent = elapsedPercent(group, now);
            return;

        case CooldownRenderMode.ChargeRing:
            if (group.maxCharges > 1) {
                options.chargeCount = group.currentCharges;
            }
            if (group.currentCharges < group.maxCharges) {
                options.rechargeTimerDonutPercent = chargeFillPercent(group, now);
            }
            return;

        case CooldownRenderMode.ChargeSweep:
            if (group.maxCharges > 1) {
                options.chargeCount = group.currentCharges;
            }
            if (group.currentCharges < group.maxCharges) {
                options.rechargeTimerSweepPercent = chargeFillPercent(group, now);
            }
            return;
    }
}

function elapsedPercent(group: CooldownGroupDetail, now: number): number {
    const duration = group.endTime - group.startTime;
    if (duration <= 0) return 100;

    return Math.min(100, Math.max(0, ((now - group.startTime) / duration) * 100));
}

function chargeFillPercent(group: CooldownGroupDetail, now: number): number {
    const interval = chargeInterval(group, now);
    if (!interval) return 100;

    return Math.min(100, Math.max(0, (interval.intoCurrentCharge / interval.chargeIntervalMs) * 100));
}

function chargeInterval(group: CooldownGroupDetail, now: number): { chargeIntervalMs: number; intoCurrentCharge: number } | undefined {
    const totalDuration = group.endTime - group.startTime;
    if (totalDuration <= 0 || group.maxCharges <= 0) return undefined;

    const chargeIntervalMs = totalDuration / group.maxCharges;
    if (chargeIntervalMs <= 0) return undefined;

    return {chargeIntervalMs, intoCurrentCharge: (now - group.startTime) % chargeIntervalMs};
}

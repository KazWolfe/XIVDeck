import {ActionAppearance, CooldownGroupDetail, CooldownRenderMode} from "../rpc/messages/ActionAppearance";

export function isCooldownActive(appearance: ActionAppearance, now = Date.now(), trackCooldowns = true): boolean {
    if (!trackCooldowns) return false;

    return isGroupActive(appearance.cooldownDetails?.recastGroup, now)
        || isGroupActive(appearance.cooldownDetails?.rechargeGroup, now);
}

export function isGroupActive(group: CooldownGroupDetail | null | undefined, now: number): boolean {
    return group != null && group.renderMode !== CooldownRenderMode.None && now < group.endTime;
}

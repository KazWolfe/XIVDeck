export enum ActionCostType {
    Default = "Default",
    Health = "Health",
    Magic = "Magic",
    Tactical = "Tactical",
    Crafting = "Crafting",
    Gathering = "Gathering",
    JobGauge = "JobGauge",
    Ceruleum = "Ceruleum",
}

export enum CooldownRenderMode {
    None = "None",
    RecastSweep = "RecastSweep",
    ChargeRing = "ChargeRing",
    ChargeSweep = "ChargeSweep",
}

export interface CooldownGroupDetail {
    groupId: number;
    renderMode: CooldownRenderMode;

    startTime: number;
    endTime: number;

    currentCharges: number;
    maxCharges: number;

    hideTimerLabel: boolean;
}

export interface CooldownNotification {
    groupId: number;
}

export interface ActionCooldownDetail {
    actionType: string;
    actionId: number;

    recastGroup: CooldownGroupDetail | null;
    rechargeGroup?: CooldownGroupDetail | null;
}

export interface ActionAppearance {
    iconId: number;

    costText: string | null;
    costType: ActionCostType;
    costRightJustified: boolean;

    cooldownDetails: ActionCooldownDetail | null;
}

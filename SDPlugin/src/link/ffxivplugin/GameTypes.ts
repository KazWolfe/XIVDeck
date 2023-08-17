export type FFXIVAction = {
    name: string | null;
    id: number;
    type: string;
    category: string | null,
    iconId: number
    sortOrder?: number | null;
    
    cooldownGroup?: number | null | undefined;
}

export type FFXIVClass = {
    id: number,
    name: string,
    
    categoryName: string,
    sortOrder: number,
    
    iconId: number,
    iconData: string,
    
    parentClass: number,
}

export type StateMessage = {
    type: string,
    params?: unknown
}

export type HotbarUpdateMessage = { 
    type: "Hotbar",
    params?: Array<{hotbarId: number, slotId: number}>
}

export type VolumePayload = {
    volume: number,
    muted: boolean,
}

export type VolumeMessage = {
    type: string,
    channel: string,
    data: { 
        volume: number,
        muted: boolean
    }
}

export type FFXIVInitReply = {
    version: string,
    apiKey: string
}

export type FFXIVHotbarSlot = {
    hotbarId: number;
    slotId: number;

    iconId: number;
    iconData: string;
    
    cooldownGroup?: number | null | undefined;
    additionalCooldownGroup?: number | null | undefined;
}

export type CooldownUpdate = {
    groupId: number;
    
    active: boolean;
    lastActionId: number;
    
    elapsedTime: number;
    recastTime: number;
    
    maxCharges: number | null | undefined;
}

export type CooldownUpdateMessage = {
    type: "cooldownUpdate",
    data: CooldownUpdate
}
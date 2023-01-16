export type FFXIVAction = {
    name: string | null;
    id: number;
    type: string;
    category: string | null,
    iconId: number
    sortOrder?: number | null;
}

export type FFXIVClass = {
    id: number,
    name: string,
    categoryName: string,
    abbreviation: string,
    sortOrder: number,
    
    iconId: number,
    iconData: string,
    
    parentClass: number,
    hasGearset: boolean
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
}
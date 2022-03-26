export type FFXIVAction = {
    name: string | null;
    id: number;
    type: string;
    category: string | null,
    iconId: number
}

export type FFXIVClass = {
    id: number,
    name: string,
    
    categoryName: string,
    sortOrder: number,
    
    iconId: number,
    iconData: string,
    
    parentClass: number,
    hasGearset: boolean
}

export type StateMessage = {
    type: string,
    params: string | null
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
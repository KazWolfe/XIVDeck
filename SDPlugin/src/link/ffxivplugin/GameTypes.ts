export type FFXIVAction = {
    name: string | null;
    id: number;
    type: string;
    category: string | null,
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
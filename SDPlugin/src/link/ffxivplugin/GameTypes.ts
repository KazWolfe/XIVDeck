export type FFXIVAction = {
    name: string | null;
    id: number;
    type: string;
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
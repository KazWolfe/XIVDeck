export interface HotbarSlotRef {
    hotbarId: number;
    slotId: number;
}

export interface HotbarChangeNotification {
    changes?: HotbarSlotRef[];
}

export interface HotbarWatchRequest {
    slots?: HotbarSlotRef[];
}

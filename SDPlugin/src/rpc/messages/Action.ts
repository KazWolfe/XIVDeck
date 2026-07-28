import {JsonObject} from "@elgato/utils";

export interface ActionEntry extends JsonObject {
    name?: string;
    id: number;
    sortOrder?: number;
    category?: string;
    type: string;
}

export interface ActionTypeUpdateNotification {
    updatedType: string;
    actionId?: number;
}

export interface ActionTypeUpdateBatch {
    updates: ActionTypeUpdateNotification[];
}

export interface GetActionsResponse {
    actions: Record<string, ActionEntry[]>;
}

export interface GetActionsByTypeResponse {
    actions: ActionEntry[];
}

import {ActionTypeUpdateBatch} from "#/client/rpc/messages/Action";

/** Helpers for reading `Action.ActionTypeUpdate` notifications. */
export class ActionTypeUpdates {
    /** Whether `batch` may have changed action `id` of `type`: a blanket update, a type-wide one, or that action. */
    public static affects(batch: ActionTypeUpdateBatch, type: string, id: number): boolean {
        return batch.updates.some(update => !update.updatedType
            || (update.updatedType === type && (update.actionId == null || update.actionId === id)));
    }
}

import streamDeck, {Action, DialAction, KeyAction, NeoInfobarAction} from "@elgato/streamdeck";
import {JsonObject} from "@elgato/utils";
import {NoConnectionError, RpcRequestError} from "#/client/rpc/GameConnection";
import {isContractError} from "#/client/rpc/messages/RpcError";

export class ControlContext {
    public constructor(public readonly action: Action<JsonObject>) {
    }

    public get id(): string {
        return this.action.id;
    }

    public asKey<TSettings extends JsonObject>(): KeyAction<TSettings> {
        if (!this.action.isKey()) {
            throw new Error(`Action ${this.action.manifestId} (${this.id}) is not on a key.`);
        }

        return this.action as KeyAction<JsonObject> as KeyAction<TSettings>;
    }

    public asDial<TSettings extends JsonObject>(): DialAction<TSettings> {
        if (!this.action.isDial()) {
            throw new Error(`Action ${this.action.manifestId} (${this.id}) is not on a dial.`);
        }

        return this.action as DialAction<JsonObject> as DialAction<TSettings>;
    }

    public asInfobar<TSettings extends JsonObject>(): NeoInfobarAction<TSettings> {
        if (!this.action.isNeoInfobar()) {
            throw new Error(`Action ${this.action.manifestId} (${this.id}) is not on a Neo infobar.`);
        }

        return this.action as NeoInfobarAction<JsonObject> as NeoInfobarAction<TSettings>;
    }

    /** Logs a failure from this control's work, at a level matching how expected it is. */
    public logError(activity: string, err: unknown): void {
        ControlContext.logActionError(this.action, activity, err);
    }

    /** {@link logError} for callers that only have the action, e.g. the dispatcher routing input. */
    public static logActionError(action: Action<JsonObject>, activity: string, err: unknown): void {
        const where = `${activity} ${action.manifestId} (${action.id})`;

        if (err instanceof NoConnectionError) {
            streamDeck.logger.debug(`Game unavailable while ${where}.`);
        } else if (err instanceof RpcRequestError && isContractError(err.code)) {
            streamDeck.logger.warn(`RPC error ${where}:`, err);
        } else {
            streamDeck.logger.error(`Unexpected error ${where}:`, err);
        }
    }
}

import type {Endpoint} from "#/settings/types/TransportSettings";

export interface ConnectionState {
    /** Kind of endpoint the control's client is connected through, or `null` while disconnected. */
    endpointKind: Endpoint["kind"] | null;
    gameVersion: string | null;
}

export const DISCONNECTED_STATE: ConnectionState = {endpointKind: null, gameVersion: null};

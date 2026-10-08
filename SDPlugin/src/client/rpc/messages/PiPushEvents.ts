import type {ConnectionState} from "#/client/ConnectionState";

export interface GameProcessState {
    running: boolean;
}

export interface PiPushEvents {
    connectionStateChanged: ConnectionState;
    gameProcessChanged: GameProcessState;
}

import type {PiClient} from "./piClient";
import {DISCONNECTED_STATE} from "#/client/ConnectionState";
import type {ConnectionState} from "#/client/ConnectionState";
import type {GameProcessState} from "#/client/rpc/messages/PiPushEvents";

export class ConnectionStateStore {
    state: ConnectionState = $state(DISCONNECTED_STATE);

    constructor(private readonly piClient: PiClient) {
        piClient.onPush("connectionStateChanged", state => this.state = state);
    }

    async refresh(): Promise<void> {
        try {
            this.state = await this.piClient.request<ConnectionState>("getConnectionState");
        } catch (err) {
            console.warn("[ConnectionStateStore] Failed to fetch connection state:", err);
        }
    }
}

export class GameProcessStore {
    state: GameProcessState = $state({running: false});

    constructor(private readonly piClient: PiClient) {
        piClient.onPush("gameProcessChanged", state => this.state = state);
    }

    async refresh(): Promise<void> {
        try {
            this.state = await this.piClient.request<GameProcessState>("getGameProcessState");
        } catch (err) {
            console.warn("[GameProcessStore] Failed to fetch game process state:", err);
        }
    }
}

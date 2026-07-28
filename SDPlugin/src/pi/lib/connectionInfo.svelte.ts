import type {PiClient} from "./piClient";
import type {ConnectionInfo} from "../../rpc/messages/ConnectionInfo";

export const EMPTY_CONNECTION_INFO: ConnectionInfo = {gameVersion: null, transport: null, gameDetected: false};

export class ConnectionInfoStore {
    info: ConnectionInfo = $state(EMPTY_CONNECTION_INFO);

    constructor(private readonly piClient: PiClient) {
        piClient.onPush("connectionStateChanged", info => this.info = info);
    }

    async refresh(): Promise<void> {
        try {
            this.info = await this.piClient.request<ConnectionInfo>("getConnectionInfo");
        } catch (err) {
            console.warn("[ConnectionInfoStore] Failed to fetch connection info:", err);
        }
    }
}

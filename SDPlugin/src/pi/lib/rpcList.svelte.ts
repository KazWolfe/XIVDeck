import type {PiClient} from "./piClient";

export class RpcList<T> {
    value: T = $state()!;

    constructor(private readonly piClient: PiClient, private readonly command: string, private readonly fallback: T) {
        this.value = fallback;

        void this._fetch();
        piClient.onPush("connectionStateChanged", info => {
            if (info.transport) {
                void this._fetch();
            } else {
                this.value = this.fallback;
            }
        });
    }

    private async _fetch(): Promise<void> {
        try {
            this.value = await this.piClient.request<T>(this.command);
        } catch {
            // nop, server offline
        }
    }
}

export function createRpcList<T>(piClient: PiClient, command: string, fallback: T): RpcList<T> {
    return new RpcList(piClient, command, fallback);
}

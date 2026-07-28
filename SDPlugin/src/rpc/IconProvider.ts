import type {XivDeckClient} from "./XivDeckClient";

const MAX_ENTRIES = 256;

/**
 * Caching provider for icons so that the game plugin can do less work.
 */
export class IconProvider {
    private readonly _entries = new Map<number, Promise<string>>();

    constructor(private readonly client: XivDeckClient) {
        client.on("_closed", () => this.clear());
        client.on("Icon.ClearIconCache", () => this.clear());
    }

    getBase64(iconId: number): Promise<string> {
        const cached = this._entries.get(iconId);
        if (cached) {
            this._entries.delete(iconId);
            this._entries.set(iconId, cached);
            return cached;
        }

        const pending = this.client.request("Icon.GetIcon", {iconId})
            .then(icon => Buffer.from(icon.png).toString("base64"));

        this._entries.set(iconId, pending);
        this._evict();

        // Don't cache failures; the next request should retry.
        pending.catch(() => {
            if (this._entries.get(iconId) === pending) {
                this._entries.delete(iconId);
            }
        });

        return pending;
    }

    clear(): void {
        this._entries.clear();
    }

    private _evict(): void {
        while (this._entries.size > MAX_ENTRIES) {
            const oldest = this._entries.keys().next().value!;
            this._entries.delete(oldest);
        }
    }
}

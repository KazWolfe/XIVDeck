import {injectable} from "inversify";
import {GameConnection} from "../rpc/GameConnection";

@injectable()
export class IconCache implements Disposable {
    private static readonly MAX_ENTRIES = 256;

    private readonly _entries = new Map<number, Promise<string>>();

    public constructor(private readonly connection: GameConnection) {
        this.connection.closed.add(this.onInvalidated);
        this.connection.subscribe("Icon.ClearIconCache", this.onInvalidated);
    }

    public getBase64(iconId: number): Promise<string> {
        const cached = this._entries.get(iconId);
        if (cached) {
            this._entries.delete(iconId);
            this._entries.set(iconId, cached);
            return cached;
        }

        const pending = this.fetch(iconId);
        this._entries.set(iconId, pending);
        this.evict();

        return pending;
    }

    public [Symbol.dispose](): void {
        this.connection.closed.remove(this.onInvalidated);
        this.connection.unsubscribe("Icon.ClearIconCache", this.onInvalidated);
        this._entries.clear();
    }

    private onInvalidated = (): void => {
        this._entries.clear();
    };

    private async fetch(iconId: number): Promise<string> {
        try {
            const icon = await this.connection.request("Icon.GetIcon", {iconId});
            return Buffer.from(icon.png).toString("base64");
        } catch (err) {
            // Don't cache failures; the next request should retry.
            this._entries.delete(iconId);
            throw err;
        }
    }

    private evict(): void {
        while (this._entries.size > IconCache.MAX_ENTRIES) {
            this._entries.delete(this._entries.keys().next().value!);
        }
    }
}

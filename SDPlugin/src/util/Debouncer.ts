export class Debouncer {
    private _timer: ReturnType<typeof setTimeout> | undefined;

    constructor(private readonly delayMs: number, private readonly onFire: () => void) {
    }

    schedule(): void {
        if (this._timer) clearTimeout(this._timer);

        this._timer = setTimeout(() => {
            this._timer = undefined;
            this.onFire();
        }, this.delayMs);
    }

    flushNow(): void {
        if (!this._timer) return;

        this.cancel();
        this.onFire();
    }

    cancel(): void {
        if (!this._timer) return;

        clearTimeout(this._timer);
        this._timer = undefined;
    }

    get isPending(): boolean {
        return this._timer !== undefined;
    }
}

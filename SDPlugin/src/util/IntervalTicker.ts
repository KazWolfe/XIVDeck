export class IntervalTicker {
    private timer: ReturnType<typeof setInterval> | undefined;

    constructor(private readonly intervalMs: number, private readonly onTick: () => void) {
    }

    sync(active: boolean): void {
        if (active && !this.timer) {
            this.timer = setInterval(this.onTick, this.intervalMs);
        } else if (!active && this.timer) {
            clearInterval(this.timer);
            this.timer = undefined;
        }
    }

    stop(): void {
        this.sync(false);
    }
}

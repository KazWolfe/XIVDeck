import {execFile} from "child_process";
import {promisify} from "util";
import {EventEmitter} from "./EventEmitter";

const execFileAsync = promisify(execFile);

// After a nudge, poll once more after this delay in case the process was still starting up or shutting down.
const NUDGE_FOLLOW_UP_MS = 2000;

export interface ProcessWatcherEvents {
    launched: [];
    terminated: [];
    error: [err: unknown];
}

async function isProcessRunning(processName: string): Promise<boolean> {
    const target = processName.toLowerCase();

    if (process.platform === "win32") {
        const {stdout} = await execFileAsync(
            "tasklist", ["/fi", `IMAGENAME eq ${processName}`, "/fo", "csv", "/nh"], {windowsHide: true},
        );

        return stdout
            .split(/\r?\n/)
            .some(line => line.toLowerCase().startsWith(`"${target}"`));
    }

    // get comm information directly for our macOS/linux checks
    const {stdout} = await execFileAsync("ps", ["-A", "-o", "comm="]);

    return stdout
        .split(/\r?\n/)
        .some(line => line.trim().split(/[\\/]/).pop()?.toLowerCase() === target);
}

/** {@link nudge} is for callers with a faster (but less reliable) signal, such as Stream Deck's application events. */
export class ProcessWatcher {
    private readonly _events = new EventEmitter<ProcessWatcherEvents>();

    private _timer?: NodeJS.Timeout;
    private _followUpTimer?: NodeJS.Timeout;
    private _started = false;
    private _suspended = false;
    private _isRunning = false;
    private _isPolling = false;

    constructor(private readonly processName: string, private readonly pollIntervalMs = 10_000) {
    }

    public start(): void {
        this._started = true;
        this._syncTimer();
    }

    public stop(): void {
        this._started = false;
        this._syncTimer();
        clearTimeout(this._followUpTimer);
        this._followUpTimer = undefined;
    }

    /** Pause periodic polling, e.g. while a live connection already proves the process is running. */
    public suspend(): void {
        this._suspended = true;
        this._syncTimer();
    }

    public resume(): void {
        this._suspended = false;
        this._syncTimer();
    }

    /** Check now, and once more shortly after, regardless of suspension. */
    public nudge(): void {
        void this._poll();

        clearTimeout(this._followUpTimer);
        this._followUpTimer = setTimeout(() => {
            this._followUpTimer = undefined;
            void this._poll();
        }, NUDGE_FOLLOW_UP_MS);
    }

    public on<K extends keyof ProcessWatcherEvents>(event: K, listener: (...args: ProcessWatcherEvents[K]) => void): () => void {
        return this._events.on(event, listener);
    }

    public isRunning(): boolean {
        return this._isRunning;
    }

    private _syncTimer(): void {
        const shouldPoll = this._started && !this._suspended;

        if (shouldPoll && !this._timer) {
            this._timer = setInterval(() => void this._poll(), this.pollIntervalMs);
            void this._poll();
        } else if (!shouldPoll && this._timer) {
            clearInterval(this._timer);
            this._timer = undefined;
        }
    }

    private async _poll(): Promise<void> {
        if (this._isPolling) {
            return;
        }
        this._isPolling = true;

        try {
            const found = await isProcessRunning(this.processName);

            if (found && !this._isRunning) {
                this._isRunning = true;
                this._events.emit("launched");
            } else if (!found && this._isRunning) {
                this._isRunning = false;
                this._events.emit("terminated");
            }
        } catch (err) {
            this._events.emit("error", err);
        } finally {
            this._isPolling = false;
        }
    }
}

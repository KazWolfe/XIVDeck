import {execFile} from "child_process";
import {promisify} from "util";
import streamDeck from "@elgato/streamdeck";
import {injectable} from "inversify";
import {EventSource, IEvent} from "./EventSource";

const execFileAsync = promisify(execFile);

/**
 * A variant of a process watcher, used to detect (more reliably) when the game's process is running.
 * Used in favor of the Elgato notifier, as those can be a bit strange on sleep or on non-Windows machines.
 */
@injectable()
export class ProcessWatcher implements Disposable {
    private static readonly PROCESS_NAME = "ffxiv_dx11.exe";

    private static readonly NUDGE_FOLLOW_UP_MS = 2_000;
    private static readonly POLL_INTERVAL_MS = 10_000;

    private readonly _launched = new EventSource();
    private readonly _terminated = new EventSource();

    private _timer?: NodeJS.Timeout;
    private _followUpTimer?: NodeJS.Timeout;
    private _isRunning = false;
    private _isPolling = false;

    public readonly launched: IEvent = this._launched;
    public readonly terminated: IEvent = this._terminated;

    public isRunning(): boolean {
        return this._isRunning;
    }

    public start(): void {
        if (this._timer) return;

        this._timer = setInterval(this.pollSync.bind(this), ProcessWatcher.POLL_INTERVAL_MS);
        this.pollSync();
    }

    public stop(): void {
        clearInterval(this._timer);
        this._timer = undefined;
        clearTimeout(this._followUpTimer);
        this._followUpTimer = undefined;
    }

    /**
     * Place the notifier in a "fast" mode, often used, e.g., for cases where we have reason to believe something
     * has happened.
     */
    public poke(): void {
        this.pollSync();

        clearTimeout(this._followUpTimer);
        this._followUpTimer = setTimeout(this.onFollowUp.bind(this), ProcessWatcher.NUDGE_FOLLOW_UP_MS);
    }

    public [Symbol.dispose](): void {
        this.stop();
        this._launched.clear();
        this._terminated.clear();
    }

    private onFollowUp(): void {
        this._followUpTimer = undefined;
        this.pollSync();
    }

    private pollSync(): void {
        this.poll().catch((ex: Error) => {
            console.error("Poll failed", ex);
        });
    }

    private async poll(): Promise<void> {
        if (this._isPolling) return;
        this._isPolling = true;

        try {
            const found = await ProcessWatcher.isProcessRunning(ProcessWatcher.PROCESS_NAME);

            if (found && !this._isRunning) {
                streamDeck.logger.info("Detected game process launch.");
                this._isRunning = true;
                this._launched.emit();
            } else if (!found && this._isRunning) {
                streamDeck.logger.info("Detected game process termination.");
                this._isRunning = false;
                this._terminated.emit();
            }
        } catch (err) {
            streamDeck.logger.warn("Failed to check for the game process:", err);
        } finally {
            this._isPolling = false;
        }
    }

    private static async isProcessRunning(processName: string): Promise<boolean> {
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
}

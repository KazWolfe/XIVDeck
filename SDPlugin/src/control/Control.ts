import {isDeepStrictEqual} from "node:util";
import {JsonObject} from "@elgato/utils";
import {ControlContext} from "./ControlContext";
import {ClientProxy} from "./proxy/ClientProxy";
import {ConnectionState} from "#/client/ConnectionState";
import {IEvent} from "#/util/EventSource";

/**
 * The base class for every XIVDeck control. Holds any relevant business logic that makes this control, well, work.
 */
export abstract class Control<TSettings extends JsonObject> implements Disposable {
    protected settings: TSettings | undefined;

    private _rendering: Promise<void> | undefined;
    private _renderQueued = false;
    private _disposed = false;

    protected constructor(protected readonly context: ControlContext, protected readonly client: ClientProxy) {
        client.availabilityChanged.add(this.requestRender.bind(this));
    }

    public get id(): string {
        return this.context.id;
    }

    /** This control's game connection changed: its client became ready or closed, or it moved to another client. */
    public get connectionChanged(): IEvent {
        return this.client.availabilityChanged;
    }

    /** Transport and game version of this control's client while connected; both `null` otherwise. */
    public get connectionState(): ConnectionState {
        return this.client.connectionState;
    }

    /** Called with migrated settings, or `undefined` if the control has never been configured. */
    public async applySettings(settings: TSettings | undefined): Promise<void> {
        if (this._disposed) return;

        const previous = this.settings;
        this.settings = settings;

        this.onSettingsChanged(previous);
        await this.requestRender();
    }

    /**
     * Requests to start a new render cycle. If a render is already in progress, queue the request until the next
     * frame instead of racing.
     */
    public async requestRender(): Promise<void> {
        if (this._disposed) return;

        if (this._rendering) {
            this._renderQueued = true;
            return this._rendering;
        }

        this._rendering = this.renderLoop();
        try {
            await this._rendering;
        } finally {
            this._rendering = undefined;
        }
    }

    /** Handles a request from this control's property inspector. Override to add commands. */
    public async handlePiRequest(command: string, _params: unknown): Promise<unknown> {
        throw new Error(`Unsupported PI command: ${command}`);
    }

    /** Releases everything registered through {@link client}. Subclasses that own other resources extend this. */
    public [Symbol.dispose](): void {
        this._disposed = true;
        this.client[Symbol.dispose]();
    }

    protected onSettingsChanged(_previous: TSettings | undefined): void {
    }

    /**
     * Handle "rendering" this control. This includes fetching game data, performing transforms, and publishing the
     * result to the Stream Deck. In effect, the entry point for business logic.
     * Internal method, external callers should use {@link requestRender} instead.
     * @protected
     */
    protected abstract render(): Promise<void>;

    protected tryReadSeconds(): TSettings {
        if (!this.settings) {
            throw new Error("Control is not configured.");
        }

        return this.settings;
    }

    protected async saveSettings(settings: TSettings): Promise<void> {
        if (isDeepStrictEqual(settings, this.settings)) {
            return;
        }

        this.settings = settings;
        await this.context.action.setSettings(settings);
    }

    private async renderLoop(): Promise<void> {
        do {
            this._renderQueued = false;
            if (this._disposed) return;

            try {
                await this.render();
            } catch (err) {
                this.context.logError("rendering", err);
            }
        } while (this._renderQueued);
    }
}

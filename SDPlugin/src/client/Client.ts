import streamDeck from "@elgato/streamdeck";
import {injectable} from "inversify";
import {GameConnection} from "./rpc/GameConnection";
import {IconCache} from "./services/IconCache";
import {HotbarTracker} from "./services/HotbarTracker";
import {CooldownTicker} from "./services/CooldownTicker";
import {FocusState} from "./rpc/messages/GameState";
import {ServerHello} from "./rpc/messages/Connection";
import {EventSource, IEvent} from "#/util/EventSource";

/**
 * A representation of a view into the game. Maintains all relevant state for the connection and resources that will
 * attach to that game.
 *
 * Controls will not interact with this class directly; they go through their {@link ClientProxy}.
 */
@injectable()
export class Client implements Disposable {
    private readonly _connectionChanged = new EventSource();
    private readonly _focusChanged = new EventSource();
    private _lastFocusTime: number | undefined;
    private _gameVersion: string | undefined;

    /** The connection became ready (with {@link gameVersion} already known) or closed. */
    public readonly connectionChanged: IEvent = this._connectionChanged;

    public readonly focusChanged: IEvent = this._focusChanged;

    public constructor(
        public readonly connection: GameConnection,
        public readonly icons: IconCache,
        public readonly hotbars: HotbarTracker,
        public readonly cooldowns: CooldownTicker,
    ) {
        connection.ready.add(this.onReady);
        connection.closed.add(this.onClosed);
        connection.subscribe("GameState.FocusChanged", this.onFocusChanged);
    }

    /** Starts connecting; the connection keeps itself connected until this client is disposed. */
    public start(): void {
        this.connection.start();
    }

    /** The handshake completed and requests can be made. */
    public get isConnected(): boolean {
        return this.connection.isReady;
    }

    /** The game plugin's version, as reported in the handshake, while connected. */
    public get gameVersion(): string | undefined {
        return this._gameVersion;
    }

    /** When the game window was last known to have focus (unix ms, the game's clock), if ever. */
    public get lastFocusTime(): number | undefined {
        return this._lastFocusTime;
    }

    public [Symbol.dispose](): void {
        this.connection.ready.remove(this.onReady);
        this.connection.closed.remove(this.onClosed);
        this.connection.unsubscribe("GameState.FocusChanged", this.onFocusChanged);
        this._connectionChanged.clear();
        this._focusChanged.clear();

        this.cooldowns[Symbol.dispose]();
        this.hotbars[Symbol.dispose]();
        this.icons[Symbol.dispose]();
        this.connection[Symbol.dispose]();
    }

    private readonly onReady = async (hello: ServerHello): Promise<void> => {
        this._gameVersion = hello.ffxivPluginVersion;
        this._connectionChanged.emit();

        let state: FocusState;
        try {
            state = await this.connection.request("GameState.GetFocusState");
        } catch (err) {
            streamDeck.logger.warn("[Client] Failed to get focus state:", err);
            return;
        }

        this.onFocusChanged(state);
    };

    private readonly onClosed = (): void => {
        this._gameVersion = undefined;
        this._connectionChanged.emit();
    };

    private readonly onFocusChanged = (state: FocusState): void => {
        this._lastFocusTime = state.lastFocusTime ?? undefined;
        this._focusChanged.emit();
    };
}

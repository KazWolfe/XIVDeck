import streamDeck from "@elgato/streamdeck";
import {Container, injectable} from "inversify";
import {Client} from "./Client";
import {GameConnection} from "./rpc/GameConnection";
import {NamedPipeTransport} from "./rpc/transports/NamedPipeTransport";
import {UnixSocketTransport} from "./rpc/transports/UnixSocketTransport";
import {IconCache} from "./services/IconCache";
import {HotbarTracker} from "./services/HotbarTracker";
import {CooldownTicker} from "./services/CooldownTicker";
import {GlobalSettingsStore} from "#/settings/GlobalSettingsStore";
import {Endpoint, Endpoints, TransportSettings} from "#/settings/types/TransportSettings";
import {EventSource, IEvent} from "#/util/EventSource";
import {ProcessWatcher} from "#/util/ProcessWatcher";

/**
 * The central client control system. Responsible for creating and deleting clients in response to events and a fixed
 * timer.
 *
 * This service is responsible for:
 * - Polling for pipes/sockets that are available while the game is running.
 * - Merging the list of autodiscovered servers with the persistent server list.
 * - Build the appropriate `Connection` and `Client` for each server, and start it; each connection then keeps itself
 *   connected until its client is removed.
 * - Handle reporting the "last focused" client when necessary.
 * - Clean everything up and stop polling when no FFXIV processes are detected.
 */
@injectable()
export class ClientManager implements Disposable {
    private static readonly SCAN_INTERVAL_MS = 5_000;

    /**
     * All tracked clients.
     * Mapped via <connection_string> -> Client instance
     */
    private readonly _clients = new Map<string, Client>();
    private readonly _beforeClientRemoval = new EventSource<[Client]>();
    private readonly _changed = new EventSource();

    /** Timer for scans for new servers. */
    private _scanTimer?: NodeJS.Timeout;

    public readonly beforeClientRemoval: IEvent<[Client]> = this._beforeClientRemoval;

    /**
     * Generic notification that *something* about a client state has changed, and that a proxy should verify
     * that it's still pointing at the right place.
     */
    public readonly changed: IEvent = this._changed;

    public constructor(
        private readonly root: Container,
        private readonly settings: GlobalSettingsStore,
        private readonly processWatcher: ProcessWatcher,
    ) {
    }

    /**
     * Get a specific client for use by a proxy.
     * @param savedConnectionId The *config connection ID* of the client to use, or `null` for the default/global
     * client.
     */
    public getClient(savedConnectionId: string | null): Client | undefined {
        if (savedConnectionId === null) {
            return this.getDefaultClient();
        } else {
            return this.resolveClientByConfigId(savedConnectionId);
        }
    }

    /**
     * Initialize the client manager.
     * Separate step to ctor since this can trigger I/O and we don't want to block the DI construction.
     */
    public initialize(): void {
        this.settings.changed.add(this.onSettingsChanged);
        this.processWatcher.launched.add(this.onLaunched);
        this.processWatcher.terminated.add(this.onTerminated);

        if (this.processWatcher.isRunning()) {
            this.wake();
        }
    }

    /** Disposes every client (closing its connection) and stops following the game and settings. */
    public [Symbol.dispose](): void {
        this.settings.changed.remove(this.onSettingsChanged);
        this.processWatcher.launched.remove(this.onLaunched);
        this.processWatcher.terminated.remove(this.onTerminated);

        this.sleep();

        this._beforeClientRemoval.clear();
        this._changed.clear();
    }

    private get isScanning(): boolean {
        return this._scanTimer !== undefined;
    }

    private readonly onLaunched = (): void => {
        streamDeck.logger.debug("[ClientManager] Game launched, connecting.");
        this.wake();
    };

    private readonly onTerminated = (): void => {
        streamDeck.logger.debug("[ClientManager] Game terminated, disconnecting.");
        this.sleep();
    };

    private readonly onSettingsChanged = (): void => {
        if (this.isScanning) {
            this.refresh();
        }

        this._changed.emit();
    };

    /** A client connected, disconnected, or reported focus, any of which can change which client is the default. */
    private readonly onClientStateChanged = (): void => {
        this._changed.emit();
    };

    /**
     * The most recently focused connected client. Never-focused clients rank last; ties go to discovery order.
     */
    private getDefaultClient(): Client | undefined {
        let best: Client | undefined;
        let bestTime = -Infinity;

        // _clients is in insertion order, i.e. the order endpoints were first discovered.
        for (const client of this._clients.values()) {
            if (!client.isConnected) continue;

            const time = client.lastFocusTime ?? -Infinity;
            if (best === undefined || time > bestTime) {
                best = client;
                bestTime = time;
            }
        }

        return best;
    }

    private resolveClientByConfigId(savedConnectionId: string): Client | undefined {
        const saved = this.settings.current.transport.savedConnections.find(c => c.id === savedConnectionId);

        return saved ? this.findClient(saved.endpoint) : undefined;
    }

    private findClient(endpoint: Endpoint): Client | undefined {
        return this._clients.get(Endpoints.keyOf(endpoint));
    }

    /** A game is running: create the clients (each starts connecting) and start rescanning. No-op if awake. */
    private wake(): void {
        if (this.isScanning) return;

        this._scanTimer = setInterval(this.refresh.bind(this), ClientManager.SCAN_INTERVAL_MS);
        this.refresh();
    }

    /** No game is running: stop rescanning and dispose every client. */
    private sleep(): void {
        clearInterval(this._scanTimer);
        this._scanTimer = undefined;

        this.sync([]);
    }

    /** Rescans (discovery, scan paths, and saved connections may all have changed) and syncs to the result. */
    private refresh(): void {
        const transport = this.settings.current.transport;
        const scanned = transport.discovery.enabled ? ClientManager.scan(transport) : [];

        this.sync(this.findCandidates(scanned));
    }

    /**
     * Synchronizes our client list with the specified candidates.
     * @param candidates The candidates to load.
     * @private
     */
    private sync(candidates: readonly Endpoint[]): void {
        const wanted = new Set(candidates.map(endpoint => Endpoints.keyOf(endpoint)));
        let changed = false;

        for (const [key, client] of [...this._clients]) {
            if (!wanted.has(key)) {
                this.destroyClient(key, client);
                changed = true;
            }
        }

        for (const endpoint of candidates) {
            if (!this.findClient(endpoint)) {
                this.buildClient(endpoint);
                changed = true;
            }
        }

        if (changed) {
            this._changed.emit();
        }
    }

    /**
     * Every candidate endpoint, without duplicates, in discovery order: saved connections in list order, then scanned
     * endpoints (oldest first) that aren't already saved.
     */
    private findCandidates(scanned: readonly Endpoint[]): Endpoint[] {
        const saved = this.settings.current.transport.savedConnections.map(connection => connection.endpoint);
        const seen = new Set<string>();
        const found: Endpoint[] = [];

        for (const endpoint of [...saved, ...scanned]) {
            const key = Endpoints.keyOf(endpoint);
            if (seen.has(key)) continue;

            seen.add(key);
            found.push(endpoint);
        }

        return found;
    }

    /** Pipes (Windows) or sockets in the configured scan paths (elsewhere) that a game may be listening on. */
    private static scan(settings: TransportSettings): Endpoint[] {
        if (process.platform === "win32") {
            return NamedPipeTransport.discover().map(name => ({kind: "pipe", name}));
        }

        return UnixSocketTransport.discover(settings.discovery.udsScanPaths).map(path => ({kind: "uds", path}));
    }

    private buildClient(endpoint: Endpoint): void {
        const scope = new Container({parent: this.root});
        scope.bind(Client).toSelf().inSingletonScope();
        scope.bind(GameConnection).toConstantValue(new GameConnection(endpoint));
        scope.bind(IconCache).toSelf().inSingletonScope();
        scope.bind(HotbarTracker).toSelf().inSingletonScope();
        scope.bind(CooldownTicker).toSelf().inSingletonScope();

        const client = scope.get(Client);
        this._clients.set(Endpoints.keyOf(endpoint), client);
        client.focusChanged.add(this.onClientStateChanged);
        client.connectionChanged.add(this.onClientStateChanged);
        client.start();
    }

    private destroyClient(key: string, client: Client): void {
        this._clients.delete(key);
        client.focusChanged.remove(this.onClientStateChanged);
        client.connectionChanged.remove(this.onClientStateChanged);

        this._beforeClientRemoval.emit(client);
        client[Symbol.dispose]();
    }
}

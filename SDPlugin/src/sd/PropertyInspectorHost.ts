import {injectable} from "inversify";
import streamDeck, {
    PropertyInspectorDidAppearEvent,
    PropertyInspectorDidDisappearEvent,
    SendToPluginEvent,
} from "@elgato/streamdeck";
import {DisposableStack, JsonObject, JsonValue} from "@elgato/utils";
import {Control} from "#/control/Control";
import {ControlDispatcher} from "#/control/ControlDispatcher";
import {ConnectionState, DISCONNECTED_STATE} from "#/client/ConnectionState";
import {GameProcessState, PiPushEvents} from "#/client/rpc/messages/PiPushEvents";
import {ProcessWatcher} from "#/util/ProcessWatcher";

interface IPiRequest {
    id: number;
    command: string;
    params?: unknown;
}

/**
 * Router for the PI to communicate with the SD side.
 * Due to business logic constraints, we no longer allow the PI to talk to the game directly, so everything must
 * be marshaled through the plugin.
 */
@injectable()
export class PropertyInspectorHost implements Disposable {
    private readonly _subscriptions = new DisposableStack();

    private _openControl: Control<JsonObject> | undefined;

    public constructor(private readonly dispatcher: ControlDispatcher, private readonly processWatcher: ProcessWatcher) {
    }

    public initialize(): void {
        this._subscriptions.use(streamDeck.ui.onDidAppear(this.onDidAppear.bind(this)));
        this._subscriptions.use(streamDeck.ui.onDidDisappear(this.onDidDisappear.bind(this)));
        this._subscriptions.use(streamDeck.ui.onSendToPlugin(this.onSendToPlugin.bind(this)));

        // we want to display different warnings if _any_ game process is not found vs the transport we're on is
        // dead.
        this.processWatcher.launched.add(this.onGameProcessChanged);
        this.processWatcher.terminated.add(this.onGameProcessChanged);
    }

    public [Symbol.dispose](): void {
        this._subscriptions.dispose();
        this.processWatcher.launched.remove(this.onGameProcessChanged);
        this.processWatcher.terminated.remove(this.onGameProcessChanged);
        this.follow(undefined);
    }

    private async onDidAppear(ev: PropertyInspectorDidAppearEvent<JsonObject>): Promise<void> {
        this.follow(this.dispatcher.get(ev.action.id));
        await this.onConnectionStateChanged();
        await this.onGameProcessChanged();
    }

    private onDidDisappear(ev: PropertyInspectorDidDisappearEvent<JsonObject>): void {
        if (this._openControl?.id === ev.action.id) {
            this.follow(undefined);
        }
    }

    /**
     * Set the control whose PI is now open. The host only ever needs to really support a single control, so we cheat.
     */
    private follow(control: Control<JsonObject> | undefined): void {
        this._openControl?.connectionChanged.remove(this.onConnectionStateChanged);
        this._openControl = control;
        this._openControl?.connectionChanged.add(this.onConnectionStateChanged);
    }

    private readonly onConnectionStateChanged = async (): Promise<void> => {
        await this.push("connectionStateChanged", this.connectionStateFor(this._openControl));
    };

    private readonly onGameProcessChanged = async (): Promise<void> => {
        await this.push("gameProcessChanged", this.gameProcessState());
    };

    private async onSendToPlugin(ev: SendToPluginEvent<JsonValue, JsonObject>): Promise<void> {
        const {id, command, params} = ev.payload as unknown as IPiRequest;

        try {
            const result = await this.runCommand(ev.action.id, command, params);
            await streamDeck.ui.sendToPropertyInspector({id, result} as JsonValue);
        } catch (err) {
            const message = err instanceof Error ? err.message : String(err);
            await streamDeck.ui.sendToPropertyInspector({id, error: message});
        }
    }

    /** Command dispatcher for PI. */
    private async runCommand(actionId: string, command: string, params: unknown): Promise<unknown> {
        const control = this.dispatcher.get(actionId);

        if (command === "getConnectionState") {
            return this.connectionStateFor(control);
        }

        if (command === "getGameProcessState") {
            return this.gameProcessState();
        }

        if (!control) {
            throw new Error(`No control exists for action ${actionId}.`);
        }

        return await control.handlePiRequest(command, params);
    }

    private connectionStateFor(control: Control<JsonObject> | undefined): ConnectionState {
        return control?.connectionState ?? DISCONNECTED_STATE;
    }

    private gameProcessState(): GameProcessState {
        return {running: this.processWatcher.isRunning()};
    }

    private async push<K extends keyof PiPushEvents>(event: K, data: PiPushEvents[K]): Promise<void> {
        try {
            await streamDeck.ui.sendToPropertyInspector({event, data} as unknown as JsonValue);
        } catch (err) {
            streamDeck.logger.warn(`Couldn't push event "${event}" to the PI.`, err, data);
        }
    }
}

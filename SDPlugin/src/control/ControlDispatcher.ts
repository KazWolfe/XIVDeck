import streamDeck, {DidReceiveSettingsEvent, SendToPluginEvent, WillAppearEvent} from "@elgato/streamdeck";
import {JsonObject, JsonValue} from "@elgato/utils";
import {ClientProvider} from "../rpc/ClientProvider";
import {GlobalSettingsProvider} from "../settings/GlobalSettingsProvider";
import {getGlobalPiCommand} from "../rpc/GlobalPiCommands";
import {BaseControl} from "./BaseControl";
import {InteractiveControl} from "./InteractiveControl";
import {HotbarButton} from "./controls/HotbarButton";
import {CommandButton} from "./controls/CommandButton";
import {ExecButton} from "./controls/ExecButton";
import {MacroButton} from "./controls/MacroButton";
import {ClassButton} from "./controls/ClassButton";
import {VolumeControl} from "./controls/VolumeControl";
import {HotbarWatchRegistry} from "../rpc/HotbarWatchRegistry";

type ControlFactory = (ev: WillAppearEvent<any>) => BaseControl<any>;

export class ControlDispatcher {
    private readonly _contextCache = new Map<string, BaseControl<any>>();
    private readonly _hotbarWatches: HotbarWatchRegistry;

    /**
     * bind our action IDs to actual, well, actions.
     */
    private readonly _factories: Record<string, ControlFactory> = {
        "dev.wolf.xivdeck.sdplugin.actions.sendcommand": ev => new CommandButton(ev, this.clients),
        "dev.wolf.xivdeck.sdplugin.actions.exechotbar": ev => new HotbarButton(ev, this.clients, this.globalSettings, this._hotbarWatches),
        "dev.wolf.xivdeck.sdplugin.actions.execaction": ev => new ExecButton(ev, this.clients, this.globalSettings),
        "dev.wolf.xivdeck.sdplugin.actions.execmacro": ev => new MacroButton(ev, this.clients, this.globalSettings),
        "dev.wolf.xivdeck.sdplugin.actions.switchclass": ev => new ClassButton(ev, this.clients),
        "dev.wolf.xivdeck.sdplugin.actions.volume": ev => new VolumeControl(ev, this.clients),
    };

    constructor(private readonly clients: ClientProvider, private readonly globalSettings: GlobalSettingsProvider) {
        this._hotbarWatches = new HotbarWatchRegistry(clients);
    }

    initialize(): void {
        streamDeck.actions.onWillAppear(ev => void this._constructControl(ev));
        streamDeck.actions.onWillDisappear(ev => this._destructControl(ev.action.id));

        streamDeck.actions.onKeyDown(ev => void this._withControl(ev.action.id, c => c.onKeyDown(ev)));
        streamDeck.actions.onDialDown(ev => void this._withControl(ev.action.id, c => c.onDialDown(ev)));
        streamDeck.actions.onTouchTap(ev => void this._withControl(ev.action.id, c => c.onTouchTap(ev)));
        streamDeck.actions.onDialRotate(ev => void this._withControl(ev.action.id, c => c.onDialRotate(ev)));

        streamDeck.settings.onDidReceiveSettings(ev => void this._handleReceivedSettings(ev));
        streamDeck.ui.onSendToPlugin(ev => void this._handleSendToPlugin(ev));
    }

    async refreshAll(): Promise<void> {
        for (const control of this._contextCache.values()) {
            await control.safeRender();
        }
    }

    private async _constructControl(ev: WillAppearEvent<JsonObject>): Promise<void> {
        const manifestId = ev.action.manifestId?.toLowerCase() ?? "";
        const factory = this._factories[manifestId];

        if (!factory) {
            streamDeck.logger.warn(`ControlDispatcher: no control for manifestId "${manifestId}", ignoring.`);
            return;
        }

        try {
            const control = factory(ev);
            this._contextCache.set(ev.action.id, control);
            await control.loadSettings(ev.payload.settings, true);
        } catch (err) {
            // e.g. an action placed on a controller its control class doesn't support.
            streamDeck.logger.error(`ControlDispatcher: failed to create ${ev.action.controllerType} control for "${manifestId}":`, err);
        }
    }

    private _destructControl(context: string): void {
        const control = this._contextCache.get(context);
        if (!control) return;

        control.cleanup();
        this._contextCache.delete(context);
    }

    private async _withControl(context: string, fn: (control: InteractiveControl<any>) => Promise<void>): Promise<void> {
        const control = this._contextCache.get(context);
        if (!(control instanceof InteractiveControl)) {
            streamDeck.logger.warn(`ControlDispatcher: no cached interactive control for context ${context}.`);
            return;
        }

        try {
            await fn(control);
        } catch (err) {
            await control.handleTriggerError(err);
        }
    }

    private async _handleReceivedSettings(ev: DidReceiveSettingsEvent<JsonObject>): Promise<void> {
        const control = this._contextCache.get(ev.action.id);
        if (!control) return;

        await control.loadSettings(ev.payload.settings, false);
    }

    private async _handleSendToPlugin(ev: SendToPluginEvent<JsonValue, JsonObject>): Promise<void> {
        const {id, command, params} = ev.payload as { id: number; command: string; params?: unknown };

        try {
            const result = await this._runPiCommand(ev.action.id, command, params);
            await streamDeck.ui.sendToPropertyInspector({id, result} as JsonValue);
        } catch (err) {
            const message = err instanceof Error ? err.message : String(err);
            await streamDeck.ui.sendToPropertyInspector({id, error: message} as JsonValue);
        }
    }

    /** Global commands win; anything else goes to the control behind the property inspector's context. */
    private async _runPiCommand(context: string, command: string, params: unknown): Promise<unknown> {
        const globalHandler = getGlobalPiCommand(command);
        if (globalHandler) return globalHandler(params);

        const control = this._contextCache.get(context);
        if (!control) throw new Error(`No control cached for context ${context}.`);

        return control.handlePiRequest(command, params);
    }
}

import {injectable} from "inversify";
import streamDeck, {ApplicationDidLaunchEvent, ApplicationDidTerminateEvent, DidReceiveGlobalSettingsEvent} from "@elgato/streamdeck";
import {DisposableStack} from "@elgato/utils";
import {ClientManager} from "./client/ClientManager";
import {ControlDispatcher} from "./control/ControlDispatcher";
import {PropertyInspectorHost} from "./sd/PropertyInspectorHost";
import {I18n} from "./i18n/i18n";
import {GlobalSettings} from "./settings/GlobalSettings";
import {GlobalSettingsStore} from "./settings/GlobalSettingsStore";
import {SettingsMigrator} from "./settings/SettingsMigrator";
import {GlobalSettingsMigrations} from "./settings/migrations/GlobalSettingsMigrations";
import {ProcessWatcher} from "./util/ProcessWatcher";

@injectable()
export class XivDeckPlugin implements Disposable {
    private readonly _subscriptions = new DisposableStack();

    public constructor(
        private readonly dispatcher: ControlDispatcher,
        private readonly piHost: PropertyInspectorHost,
        private readonly clientManager: ClientManager,
        private readonly processWatcher: ProcessWatcher,
        private readonly globalSettings: GlobalSettingsStore,
    ) {
    }

    public async start(): Promise<void> {
        this.dispatcher.initialize();
        this.piHost.initialize();

        await streamDeck.connect();

        await I18n.initialize(streamDeck.info.application.language);

        // retrieve and migrate (if necessary) global settings.
        await this.initializeGlobalSettings();

        this._subscriptions.use(
            streamDeck.settings.onDidReceiveGlobalSettings<GlobalSettings>(this.onDidReceiveGlobalSettings.bind(this)));
        this._subscriptions.use(streamDeck.system.onApplicationDidLaunch(this.onApplicationDidLaunch.bind(this)));
        this._subscriptions.use(streamDeck.system.onApplicationDidTerminate(this.onApplicationDidTerminate.bind(this)));

        this.clientManager.start();
        this.processWatcher.start();
    }

    public [Symbol.dispose](): void {
        this._subscriptions.dispose();
        this.piHost[Symbol.dispose]();
        this.dispatcher[Symbol.dispose]();
        this.clientManager[Symbol.dispose]();
        this.processWatcher[Symbol.dispose]();
    }

    private async onDidReceiveGlobalSettings(ev: DidReceiveGlobalSettingsEvent<GlobalSettings>): Promise<void> {
        try {
            this.globalSettings.update(ev.settings);
        } catch (err) {
            streamDeck.logger.error("Failed to apply global settings:", err);
        }
    }

    private async initializeGlobalSettings(): Promise<void> {
        const oldSettings = await streamDeck.settings.getGlobalSettings();

        const migrated = SettingsMigrator.migrate<GlobalSettings>(GlobalSettingsMigrations, oldSettings);
        if (migrated !== oldSettings) {
            await streamDeck.settings.setGlobalSettings(migrated);
        }

        this.globalSettings.update(migrated);
    }

    private onApplicationDidLaunch(ev: ApplicationDidLaunchEvent): void {
        streamDeck.logger.debug(`Stream Deck reported application launch: ${ev.application}`);
        this.processWatcher.poke();
    }

    private onApplicationDidTerminate(ev: ApplicationDidTerminateEvent): void {
        streamDeck.logger.debug(`Stream Deck reported application termination: ${ev.application}`);
        this.processWatcher.poke();
    }
}

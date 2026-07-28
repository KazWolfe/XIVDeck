import streamDeck from "@elgato/streamdeck";

import "./settings/migrations/GlobalSettingsMigrations";
import "./settings/migrations/HotbarSettingsMigrations";
import "./settings/migrations/CommandSettingsMigrations";
import "./settings/migrations/ExecActionSettingsMigrations";
import "./settings/migrations/MacroSettingsMigrations";
import "./settings/migrations/ClassSettingsMigrations";
import "./settings/migrations/VolumeSettingsMigrations";

import {SettingsGateway} from "./settings/SettingsGateway";
import {DEFAULT_TRANSPORT_ID, GlobalSettings} from "./settings/GlobalSettings";
import {GlobalSettingsStore} from "./settings/GlobalSettingsProvider";
import {XivDeckClient} from "./rpc/XivDeckClient";
import {ClientRegistry} from "./rpc/ClientProvider";
import {makeTransportResolver, transportsEqual} from "./rpc/TransportDiscovery";
import {ControlDispatcher} from "./control/ControlDispatcher";
import i18n from "./i18n/i18n";
import {registerGlobalPiCommand} from "./rpc/GlobalPiCommands";
import {pushToPropertyInspector} from "./rpc/PiPush";
import {ConnectionInfo} from "./rpc/messages/ConnectionInfo";
import {ProcessWatcher} from "./util/ProcessWatcher";
import {EventEmitter} from "./util/EventEmitter";

const FFXIV_PROCESS_NAME = "ffxiv_dx11.exe";

async function main(): Promise<void> {
    EventEmitter.onListenerError = (event, err) => {
        streamDeck.logger.error(`Listener for "${String(event)}" failed:`, err);
    };

    const clients = new ClientRegistry();
    const client = new XivDeckClient();
    clients.addClient(DEFAULT_TRANSPORT_ID, client);

    const globalSettings = new GlobalSettingsStore();
    const dispatcher = new ControlDispatcher(clients, globalSettings);
    dispatcher.initialize();

    const processWatcher = new ProcessWatcher(FFXIV_PROCESS_NAME);

    const getConnectionInfo = (): ConnectionInfo => ({
        gameVersion: client.gameVersion ?? null,
        transport: client.isReady() ? client.transportLabel ?? null : null,
        gameDetected: processWatcher.isRunning(),
    });

    const broadcastConnectionState = () =>
        void pushToPropertyInspector("connectionStateChanged", getConnectionInfo());

    client.on("_ready", () => {
        processWatcher.suspend();
        void dispatcher.refreshAll();
        broadcastConnectionState();
    });
    client.on("_closed", () => {
        processWatcher.resume();
        void dispatcher.refreshAll();
        broadcastConnectionState();
    });

    registerGlobalPiCommand("getConnectionInfo", () => getConnectionInfo());

    const connect = () => {
        const current = globalSettings.getSettings();
        return client.connect(makeTransportResolver(current.transports, current.chosenTransport));
    };

    // NOTE: We need to bring this up ASAP, since everything else depends on our link back to the SD being operational.
    await streamDeck.connect();

    await i18n.changeLanguage(streamDeck.info.application.language);

    const raw = await streamDeck.settings.getGlobalSettings<GlobalSettings>();
    const loaded = SettingsGateway.load<GlobalSettings>("global", raw);
    if (loaded !== raw) {
        await streamDeck.settings.setGlobalSettings(loaded);
    }
    globalSettings.update(loaded);

    streamDeck.settings.onDidReceiveGlobalSettings<GlobalSettings>(async ev => {
        const migrated = SettingsGateway.load<GlobalSettings>("global", ev.settings);
        if (migrated !== ev.settings) {
            await streamDeck.settings.setGlobalSettings(migrated);
        }

        const previous = globalSettings.getSettings();
        globalSettings.update(migrated);

        // only reconnect if the transport configuration actually changed
        if (!transportsEqual(previous.transports, migrated.transports) || previous.chosenTransport !== migrated.chosenTransport) {
            client.shutdown();

            if (processWatcher.isRunning()) await connect();
        }
    });

    processWatcher.on("launched", () => {
        streamDeck.logger.info(`Detected process launch: ${FFXIV_PROCESS_NAME}`);
        void connect();
        broadcastConnectionState();
    });

    processWatcher.on("terminated", () => {
        streamDeck.logger.info(`Detected process termination: ${FFXIV_PROCESS_NAME}`);
        client.shutdown();
        broadcastConnectionState();
    });

    processWatcher.on("error", err => {
        streamDeck.logger.warn(`Failed to check for process ${FFXIV_PROCESS_NAME}:`, err);
    });

    streamDeck.system.onApplicationDidLaunch(ev => {
        streamDeck.logger.debug(`Stream Deck reported application launch: ${ev.application}`);
        processWatcher.nudge();
    });
    streamDeck.system.onApplicationDidTerminate(ev => {
        streamDeck.logger.debug(`Stream Deck reported application termination: ${ev.application}`);
        processWatcher.nudge();
    });

    processWatcher.start();
}

main().catch(err => {
    console.error("[plugin] Fatal error during startup:", err);
    process.exitCode = 1;
});

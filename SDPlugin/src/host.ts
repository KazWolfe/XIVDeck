import "reflect-metadata";
import streamDeck from "@elgato/streamdeck";
import {Container} from "inversify";
import {ClientManager} from "./client/ClientManager";
import {ControlDispatcher} from "./control/ControlDispatcher";
import {ControlFactory} from "./control/ControlFactory";
import {HotbarSlotRenderer} from "./control/virtual_slot/HotbarSlotRenderer";
import {PropertyInspectorHost} from "./sd/PropertyInspectorHost";
import {GlobalSettingsStore} from "./settings/GlobalSettingsStore";
import {ProcessWatcher} from "./util/ProcessWatcher";
import {XivDeckPlugin} from "./XivDeckPlugin";

async function main(): Promise<void> {
    const root = new Container();
    root.bind(Container).toConstantValue(root);

    root.bind(XivDeckPlugin).toSelf().inSingletonScope();
    root.bind(GlobalSettingsStore).toSelf().inSingletonScope();
    root.bind(ProcessWatcher).toSelf().inSingletonScope();
    root.bind(PropertyInspectorHost).toSelf().inSingletonScope();
    root.bind(HotbarSlotRenderer).toSelf().inSingletonScope();
    root.bind(ClientManager).toSelf().inSingletonScope();
    root.bind(ControlDispatcher).toSelf().inSingletonScope();
    root.bind(ControlFactory).toSelf().inSingletonScope();

    const plugin = root.get(XivDeckPlugin);

    process.once("SIGINT", signal => shutdown(plugin, signal));
    process.once("SIGTERM", signal => shutdown(plugin, signal));

    await plugin.start();
}

function shutdown(plugin: XivDeckPlugin, signal: NodeJS.Signals): void {
    streamDeck.logger.info(`Received ${signal}, shutting down.`);

    try {
        plugin[Symbol.dispose]();
    } catch (err) {
        streamDeck.logger.error("Error during shutdown:", err);
        process.exitCode = 1;
    }

    // The Stream Deck socket would otherwise keep the process alive.
    process.exit();
}

main().catch(err => {
    streamDeck.logger.error("Fatal error during startup:", err);
    console.error("[plugin] Fatal error during startup:", err);
    process.exitCode = 1;
});

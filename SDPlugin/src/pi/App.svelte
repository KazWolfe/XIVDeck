<script lang="ts">
    import {onMount} from "svelte";
    import type {JsonObject} from "@elgato/utils";
    import type {Component} from "svelte";
    import {StreamDeckSocket} from "./lib/StreamDeckSocket";
    import {PiClient} from "./lib/piClient";
    import {ConnectionInfoStore, EMPTY_CONNECTION_INFO} from "./lib/connectionInfo.svelte";
    import {setLanguage, t} from "./lib/i18n.svelte";
    import {DefaultGlobalSettings} from "../settings/GlobalSettings";
    import type {GlobalSettings} from "../settings/GlobalSettings";
    import HotbarFrame from "./frames/HotbarFrame.svelte";
    import CommandFrame from "./frames/CommandFrame.svelte";
    import ExecActionFrame from "./frames/ExecActionFrame.svelte";
    import MacroFrame from "./frames/MacroFrame.svelte";
    import ClassFrame from "./frames/ClassFrame.svelte";
    import VolumeFrame from "./frames/VolumeFrame.svelte";
    import GlobalFrame from "./frames/GlobalFrame.svelte";
    import NoGameBanner from "./banners/NoGameBanner.svelte";
    import GameDisconnectedBanner from "./banners/GameDisconnectedBanner.svelte";


    const FRAMES: Record<string, Component<any>> = {
        "dev.wolf.xivdeck.sdplugin.actions.exechotbar": HotbarFrame,
        "dev.wolf.xivdeck.sdplugin.actions.sendcommand": CommandFrame,
        "dev.wolf.xivdeck.sdplugin.actions.execaction": ExecActionFrame,
        "dev.wolf.xivdeck.sdplugin.actions.execmacro": MacroFrame,
        "dev.wolf.xivdeck.sdplugin.actions.switchclass": ClassFrame,
        "dev.wolf.xivdeck.sdplugin.actions.volume": VolumeFrame,
    };

    let socket: StreamDeckSocket<JsonObject> | undefined = $state();
    let piClient: PiClient | undefined = $state();
    let settings: JsonObject | undefined = $state();
    let globalSettings: GlobalSettings = $state(DefaultGlobalSettings);
    let frame: Component<any> | undefined = $state();

    let connectionStore: ConnectionInfoStore | undefined = $state();
    let connectionInfo = $derived(connectionStore?.info ?? EMPTY_CONNECTION_INFO);

    onMount(async () => {
        const s = await StreamDeckSocket.waitForConnection<JsonObject>();

        socket = s;
        piClient = new PiClient(s);
        settings = s.actionInfo.payload.settings;
        frame = FRAMES[s.actionInfo.action.toLowerCase()];
        setLanguage(s.info.application.language);

        s.onDidReceiveSettings(newSettings => settings = newSettings);
        s.onDidReceiveGlobalSettings(newSettings => globalSettings = newSettings as GlobalSettings);

        s.getGlobalSettings();

        connectionStore = new ConnectionInfoStore(piClient);
        await connectionStore.refresh();
    });

    function handleSettingsChange(newSettings: JsonObject): void {
        settings = newSettings;
        socket?.setSettings(newSettings);
    }

    function handleGlobalSettingsChange(newSettings: GlobalSettings): void {
        globalSettings = newSettings;
        socket?.setGlobalSettings(newSettings);
    }
</script>

<div class="sdpi-wrapper">
    {#if !connectionInfo.transport}
        {#if socket}
            {#if connectionInfo.gameDetected}
                <GameDisconnectedBanner {socket}/>
            {:else}
                <NoGameBanner {socket}/>
            {/if}
        {/if}
    {:else}
        <div class="sdpi-heading">{t("pi:App.actionHeader")}</div>

        {#if frame && settings && socket}
            {@const Frame = frame}
            <Frame
                {settings}
                onSettingsChange={handleSettingsChange}
                {piClient}
                controller={socket.actionInfo.payload.controller}
            />
        {:else if socket}
            <p>{t("pi:App.unsupportedAction", {action: socket.actionInfo.action})}</p>
        {:else}
            <p>{t("pi:App.connecting")}</p>
        {/if}
    {/if}

    <div class="sdpi-heading">{t("pi:App.pluginHeader")}</div>
    <GlobalFrame
        settings={globalSettings}
        onSettingsChange={handleGlobalSettingsChange}
        activeTransport={connectionInfo.transport}
        platform={socket?.info.application.platform ?? ""}
    />

    <div class="version-info">
        <small>
            <span>{t("pi:App.buildVersion")} <code>{__BUILD_VERSION__}</code></span> &bull;
            <span>{t("pi:App.runtimeVersion")} <code>{socket?.info.application.version ?? "???"}</code></span><br>
            <span>{t("pi:App.gamePluginVersion")} <code>{connectionInfo.gameVersion ?? "???"}</code></span>
        </small>
    </div>
</div>

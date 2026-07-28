<script lang="ts">
    import {DEFAULT_TRANSPORT_ID} from "../../settings/GlobalSettings";
    import type {GlobalSettings} from "../../settings/GlobalSettings";
    import type {AnyTransportConfig} from "../../settings/types/TransportConfig";
    import {DEFAULT_WS_PORT} from "../../rpc/transports/WebSocketTransport";
    import {t} from "../lib/i18n.svelte";


    let {settings, onSettingsChange, activeTransport, platform}: {
        settings: GlobalSettings;
        onSettingsChange: (settings: GlobalSettings) => void;
        activeTransport: string | null;
        platform: string;
    } = $props();

    type TransportSelection = "autodetect" | "websocket" | "namedPipe" | "uds";

    let socketDirectoryInput: HTMLInputElement | undefined = $state();

    let defaultTransport = $derived(settings.transports.find(t => t.id === DEFAULT_TRANSPORT_ID));

    let socketDirectory = $derived(defaultTransport?.type === "uds" ? defaultTransport.filename : undefined);

    let selection: TransportSelection = $derived(
        settings.chosenTransport === DEFAULT_TRANSPORT_ID && defaultTransport ? defaultTransport.type : "autodetect",
    );

    // Keyed by the prefix of the client's transport label ("pipe:...", "ws:...", "uds:...").
    const TRANSPORT_NAME_KEYS: Record<string, string> = {
        pipe: "pi:GlobalFrame.namedPipe",
        ws: "pi:GlobalFrame.webSocket",
        uds: "pi:GlobalFrame.unixSocket",
    };

    let activeTransportLabel = $derived.by(() => {
        if (!activeTransport) return t("pi:GlobalFrame.notConnected");

        const key = TRANSPORT_NAME_KEYS[activeTransport.split(":", 1)[0]];
        return key ? t(key) : activeTransport;
    });

    function setDefaultTransport(transport: AnyTransportConfig | null, chosenTransport: string | null) {
        onSettingsChange({
            ...settings,
            chosenTransport,
            transports: transport
                ? [transport, ...settings.transports.filter(t => t.id !== DEFAULT_TRANSPORT_ID)]
                : settings.transports,
        });
    }

    function onTransportTypeChange(ev: Event) {
        const value = (ev.target as HTMLSelectElement).value as TransportSelection;

        if (value === "autodetect") {
            onSettingsChange({...settings, chosenTransport: null});
        } else if (value === "websocket") {
            setDefaultTransport({
                id: DEFAULT_TRANSPORT_ID,
                label: "WebSocket",
                type: "websocket",
                hostname: "localhost",
                port: defaultTransport?.type === "websocket" ? defaultTransport.port : DEFAULT_WS_PORT,
            }, DEFAULT_TRANSPORT_ID);
        } else if (value === "namedPipe") {
            setDefaultTransport({
                id: DEFAULT_TRANSPORT_ID,
                label: "Named Pipe",
                type: "namedPipe",
            }, DEFAULT_TRANSPORT_ID);
        } else if (value === "uds") {
            setDefaultTransport({
                id: DEFAULT_TRANSPORT_ID,
                label: "Unix Domain Socket",
                type: "uds",
                filename: defaultTransport?.type === "uds" ? defaultTransport.filename : undefined,
            }, DEFAULT_TRANSPORT_ID);
        }
    }

    function onPortChange(ev: Event) {
        const value = (ev.target as HTMLInputElement).value;
        if (value === "" || defaultTransport?.type !== "websocket") return;

        setDefaultTransport({...defaultTransport, port: parseInt(value, 10)}, DEFAULT_TRANSPORT_ID);
    }

    // NOTE: only directory selection is supported for now, since the game recreates the socket with a new
    // name every boot, so we can't point directly at a specific socket file yet.
    function onSocketDirectoryChange(ev: Event) {
        const input = ev.target as HTMLInputElement;

        const raw = input.value;
        if (!raw) return;

        const path = decodeURIComponent(raw.replace(/^C:\\fakepath\\/, ""));

        const relative = input.files?.[0]?.webkitRelativePath;
        const rootName = relative ? relative.split("/")[0] : "";
        const rootIndex = rootName ? path.lastIndexOf(rootName) : -1;
        const dirPath = rootIndex >= 0 ? path.slice(0, rootIndex + rootName.length) : path;

        setDefaultTransport({
            id: DEFAULT_TRANSPORT_ID,
            label: "Unix Domain Socket",
            type: "uds",
            filename: dirPath,
        }, DEFAULT_TRANSPORT_ID);
    }

    function onClearSocketDirectory() {
        if (socketDirectoryInput) socketDirectoryInput.value = "";

        setDefaultTransport({
            id: DEFAULT_TRANSPORT_ID,
            label: "Unix Domain Socket",
            type: "uds",
            filename: undefined,
        }, DEFAULT_TRANSPORT_ID);
    }

    function onTrackCooldownsChange(ev: Event) {
        onSettingsChange({...settings, trackCooldowns: (ev.target as HTMLInputElement).checked});
    }
</script>

<details class="sdpi-accordion">
    <summary>
        {t("pi:GlobalFrame.connectionSettings")}
        <span class="sdpi-accordion-hint">{activeTransportLabel}</span>
    </summary>

    <div class="sdpi-item">
        <label class="sdpi-item-label" for="transportType">{t("pi:GlobalFrame.type")}</label>
        <select class="sdpi-item-value" id="transportType" value={selection} onchange={onTransportTypeChange}>
            <option value="autodetect">{t("pi:GlobalFrame.autodetect")}</option>
            <option value="websocket">{t("pi:GlobalFrame.webSocket")}</option>
            {#if platform === "windows"}
                <option value="namedPipe">{t("pi:GlobalFrame.namedPipe")}</option>
            {/if}
            {#if platform !== "windows"}
                <option value="uds">{t("pi:GlobalFrame.unixSocket")}</option>
            {/if}
        </select>
    </div>

    {#if selection === "websocket"}
        <div class="sdpi-item">
            <label class="sdpi-item-label" for="wsPort">{t("pi:GlobalFrame.port")}</label>
            <input class="sdpi-item-value" id="wsPort" type="number" min="1024" max="49151"
                   value={defaultTransport?.type === "websocket" ? defaultTransport.port : DEFAULT_WS_PORT} onchange={onPortChange}/>
        </div>
    {:else if selection === "uds"}
        <div class="sdpi-item">
            <div class="sdpi-item-label">{t("pi:GlobalFrame.searchPath")}</div>
            <div class="sdpi-item-group file">
                <input id="socketDirectory" type="file" webkitdirectory
                       bind:this={socketDirectoryInput} onchange={onSocketDirectoryChange}/>
                <label class="sdpi-file-info" for="socketDirectory" title={socketDirectory ?? t("pi:GlobalFrame.autodetect")}>
                    {socketDirectory ?? t("pi:GlobalFrame.autodetect")}
                </label>
                <label class="sdpi-file-label" for="socketDirectory">{t("pi:GlobalFrame.choose")}</label>
                {#if socketDirectory}
                    <button onclick={onClearSocketDirectory}>{t("pi:GlobalFrame.clear")}</button>
                {/if}
            </div>
        </div>
    {/if}
</details>

<div class="sdpi-item" type="checkbox">
    <div class="sdpi-item-label">{t("pi:GlobalFrame.cooldowns")}</div>
    <div class="sdpi-item-value">
        <input type="checkbox" id="trackCooldowns" checked={settings.trackCooldowns ?? true} onchange={onTrackCooldownsChange}>
        <label for="trackCooldowns"><span></span>{t("pi:GlobalFrame.trackCooldowns")}</label>
    </div>
</div>

<script lang="ts">
    import type {GlobalSettings} from "#/settings/GlobalSettings";
    import type {Endpoint, TransportSettings} from "#/settings/types/TransportSettings";
    import {ConnectionModes} from "../lib/ConnectionModes";
    import type {ConnectionMode} from "../lib/ConnectionModes";
    import {t} from "../lib/i18n.svelte";


    let {settings, onSettingsChange, endpointKind, platform}: {
        settings: GlobalSettings;
        onSettingsChange: (settings: GlobalSettings) => void;
        endpointKind: Endpoint["kind"] | null;
        platform: string;
    } = $props();

    // Shown (disabled) only when the stored settings match no mode, e.g. after a hand edit.
    const CUSTOM_SELECTION = "custom";

    let mode = $derived(ConnectionModes.derive(settings.transport));
    let selection: ConnectionMode | typeof CUSTOM_SELECTION = $derived(mode ?? CUSTOM_SELECTION);
    let scanPaths = $derived(settings.transport.discovery.udsScanPaths);

    const ENDPOINT_KIND_NAME_KEYS: Record<Endpoint["kind"], string> = {
        pipe: "pi:GlobalFrame.namedPipe",
        ws: "pi:GlobalFrame.webSocket",
        uds: "pi:GlobalFrame.unixSocket",
    };

    let endpointKindLabel = $derived(
        endpointKind ? t(ENDPOINT_KIND_NAME_KEYS[endpointKind]) : t("pi:GlobalFrame.notConnected"),
    );

    function setTransport(transport: TransportSettings) {
        onSettingsChange({...settings, transport});
    }

    function onModeChange(ev: Event) {
        const value = (ev.target as HTMLSelectElement).value as ConnectionMode;
        setTransport(ConnectionModes.apply(value, settings.transport));
    }

    function onPortChange(ev: Event) {
        const value = parseInt((ev.target as HTMLInputElement).value, 10);
        if (Number.isNaN(value)) return;

        setTransport(ConnectionModes.withPort(settings.transport, value));
    }

    // NOTE: only directories can be added, since the game recreates the socket with a new name every boot, so we
    // can't point directly at a specific socket file yet.
    function onAddScanPath(ev: Event) {
        const input = ev.target as HTMLInputElement;

        const raw = input.value;
        if (!raw) return;

        const path = decodeURIComponent(raw.replace(/^C:\\fakepath\\/, ""));

        const relative = input.files?.[0]?.webkitRelativePath;
        const rootName = relative ? relative.split("/")[0] : "";
        const rootIndex = rootName ? path.lastIndexOf(rootName) : -1;
        const dirPath = rootIndex >= 0 ? path.slice(0, rootIndex + rootName.length) : path;

        input.value = "";
        if (scanPaths.includes(dirPath)) return;

        setTransport(ConnectionModes.withScanPaths(settings.transport, [...scanPaths, dirPath]));
    }

    function onRemoveScanPath(path: string) {
        setTransport(ConnectionModes.withScanPaths(settings.transport, scanPaths.filter(p => p !== path)));
    }

    function onTrackCooldownsChange(ev: Event) {
        onSettingsChange({...settings, trackCooldowns: (ev.target as HTMLInputElement).checked});
    }
</script>

<details class="sdpi-accordion">
    <summary>
        {t("pi:GlobalFrame.connectionSettings")}
        <span class="sdpi-accordion-hint">{endpointKindLabel}</span>
    </summary>

    <div class="sdpi-item">
        <label class="sdpi-item-label" for="connectionMode">{t("pi:GlobalFrame.type")}</label>
        <select class="sdpi-item-value" id="connectionMode" value={selection} onchange={onModeChange}>
            {#if selection === CUSTOM_SELECTION}
                <option value={CUSTOM_SELECTION} disabled>{t("pi:GlobalFrame.custom")}</option>
            {/if}
            <option value="automatic">{t("pi:GlobalFrame.autodetect")}</option>
            <option value="websocket">{t("pi:GlobalFrame.webSocket")}</option>
        </select>
    </div>

    {#if selection === "websocket"}
        <div class="sdpi-item">
            <label class="sdpi-item-label" for="wsPort">{t("pi:GlobalFrame.port")}</label>
            <input class="sdpi-item-value" id="wsPort" type="number" min="1024" max="49151"
                   value={ConnectionModes.portOf(settings.transport)} onchange={onPortChange}/>
        </div>
    {:else if selection === "automatic" && platform !== "windows"}
        <div class="sdpi-item">
            <div class="sdpi-item-label">{t("pi:GlobalFrame.searchPath")}</div>
            <div class="sdpi-item-value">
                {#each scanPaths as path (path)}
                    <div class="sdpi-item-group">
                        <span class="sdpi-file-info" title={path}>{path}</span>
                        <button onclick={() => onRemoveScanPath(path)}>{t("pi:GlobalFrame.clear")}</button>
                    </div>
                {:else}
                    <small>{t("pi:GlobalFrame.searchPathDefault")}</small>
                {/each}
                <div class="sdpi-item-group file">
                    <input id="socketDirectory" type="file" webkitdirectory onchange={onAddScanPath}/>
                    <label class="sdpi-file-label" for="socketDirectory">{t("pi:GlobalFrame.addPath")}</label>
                </div>
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

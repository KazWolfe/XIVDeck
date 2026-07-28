<script lang="ts">
    import {VolumeControlMode} from "../../settings/types/VolumeControlSettings";
    import type {StoredVolumeControlSettings} from "../../settings/types/VolumeControlSettings";
    import type {ActionInfo} from "../lib/StreamDeckSocket";
    import type {PiClient} from "../lib/piClient";
    import {createRpcList} from "../lib/rpcList.svelte";
    import {t} from "../lib/i18n.svelte";

    const DEFAULT_SET_VALUE = 100;
    const DEFAULT_ADJUST_STEP = 0;

    let {settings, onSettingsChange, piClient, controller}: {
        settings: StoredVolumeControlSettings;
        onSettingsChange: (settings: StoredVolumeControlSettings) => void;
        piClient: PiClient;
        controller?: ActionInfo["payload"]["controller"];
    } = $props();

    // svelte-ignore state_referenced_locally -- piClient is fixed for the component's lifetime.
    const channelList = createRpcList<string[]>(piClient, "listChannels", []);
    let channels = $derived(channelList.value);

    let isDial = $derived(controller === "Encoder");
    let mode = $derived(settings.mode ?? VolumeControlMode.MUTE);

    let selectedChannelMissing = $derived(!!settings.channel && !channels.includes(settings.channel));

    function onChannelChange(ev: Event) {
        onSettingsChange({...settings, channel: (ev.target as HTMLSelectElement).value});
    }

    function onModeChange(ev: Event) {
        const mode = (ev.target as HTMLInputElement).value as VolumeControlMode;

        switch (mode) {
            case VolumeControlMode.SET:
                onSettingsChange({...settings, mode, value: settings.value ?? DEFAULT_SET_VALUE});
                break;
            case VolumeControlMode.ADJUST:
                onSettingsChange({...settings, mode, multiplier: settings.multiplier ?? DEFAULT_ADJUST_STEP});
                break;
            default:
                onSettingsChange({...settings, mode});
        }
    }

    function onValueChange(ev: Event) {
        const value = parseInt((ev.target as HTMLInputElement).value, 10);

        if (isDial || mode === VolumeControlMode.ADJUST) {
            onSettingsChange({...settings, multiplier: value});
        } else {
            onSettingsChange({...settings, value});
        }
    }
</script>

<div class="sdpi-item">
    <label class="sdpi-item-label" for="channelSelector">{t("pi:VolumeFrame.channel")}</label>
    <select class="sdpi-item-value" id="channelSelector" value={settings.channel ?? ""} onchange={onChannelChange}>
        <option value="" disabled>{t("pi:VolumeFrame.placeholder")}</option>
        {#if selectedChannelMissing}
            <option value={settings.channel} selected disabled>{t(`soundChannels:${settings.channel}.full`, {defaultValue: settings.channel})}</option>
        {/if}
        {#each channels as channel (channel)}
            <option value={channel}>{t(`soundChannels:${channel}.full`)}</option>
        {/each}
    </select>
</div>

{#if !isDial}
    <div class="sdpi-item" type="radio">
        <span class="sdpi-item-label">{t("pi:VolumeFrame.mode")}</span>
        <div class="sdpi-item-value">
            <input type="radio" id="volumeModeMute" name="volumeMode" value={VolumeControlMode.MUTE} checked={mode === VolumeControlMode.MUTE} onchange={onModeChange}>
            <label for="volumeModeMute"><span></span>{t("pi:VolumeFrame.mute")}</label>

            <input type="radio" id="volumeModeSet" name="volumeMode" value={VolumeControlMode.SET} checked={mode === VolumeControlMode.SET} onchange={onModeChange}>
            <label for="volumeModeSet"><span></span>{t("pi:VolumeFrame.set")}</label>

            <input type="radio" id="volumeModeAdjust" name="volumeMode" value={VolumeControlMode.ADJUST} checked={mode === VolumeControlMode.ADJUST} onchange={onModeChange}>
            <label for="volumeModeAdjust"><span></span>{t("pi:VolumeFrame.adjust")}</label>
        </div>
    </div>
{/if}

{#if isDial}
    <div class="sdpi-item">
        <label class="sdpi-item-label" for="valueField">{t("pi:VolumeFrame.step")}</label>
        <input class="sdpi-item-value" id="valueField" type="range" min="1" max="5" value={settings.multiplier ?? 1} onchange={onValueChange}/>
    </div>
{:else if mode === VolumeControlMode.SET}
    <div class="sdpi-item">
        <label class="sdpi-item-label" for="valueField">{t("pi:VolumeFrame.volume")}</label>
        <input class="sdpi-item-value" id="valueField" type="range" min="0" max="100" value={settings.value ?? DEFAULT_SET_VALUE} onchange={onValueChange}/>
    </div>
{:else if mode === VolumeControlMode.ADJUST}
    <div class="sdpi-item">
        <label class="sdpi-item-label" for="valueField">{t("pi:VolumeFrame.step")}</label>
        <input class="sdpi-item-value" id="valueField" type="range" min="-25" max="25" value={settings.multiplier ?? DEFAULT_ADJUST_STEP} onchange={onValueChange}/>
    </div>
{/if}

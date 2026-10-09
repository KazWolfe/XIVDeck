<script lang="ts">
    import {VolumeControlMode} from "#/settings/types/VolumeControlSettings";
    import type {VolumeControlSettings} from "#/settings/types/VolumeControlSettings";
    import type {ActionInfo} from "../lib/StreamDeckSocket";
    import type {PiClient} from "../lib/piClient";
    import {createRpcList} from "../lib/rpcList.svelte";
    import {t} from "../lib/i18n.svelte";

    const DEFAULT_SET_VALUE = 100;
    const DEFAULT_ADJUST_STEP = 0;
    const DEFAULT_DIAL_STEP = 1;

    let {settings, onSettingsChange, piClient, controller}: {
        settings: VolumeControlSettings | undefined;
        onSettingsChange: (settings: VolumeControlSettings) => void;
        piClient: PiClient;
        controller?: ActionInfo["payload"]["controller"];
    } = $props();

    // svelte-ignore state_referenced_locally -- piClient is fixed for the component's lifetime.
    const channelList = createRpcList<string[]>(piClient, "listChannels", []);
    let channels = $derived(channelList.value);

    let isDial = $derived(controller === "Encoder");

    // Selections not yet saved: the frame only saves once a channel is chosen. Reset to the saved settings whenever
    // those change.
    let channel = $derived(settings?.channel);
    let mode = $derived(settings?.mode ?? VolumeControlMode.MUTE);
    let setValue = $derived(settings?.mode === VolumeControlMode.SET ? settings.value : DEFAULT_SET_VALUE);
    let multiplier = $derived(settings && settings.mode !== VolumeControlMode.SET ? settings.multiplier : undefined);

    let selectedChannelMissing = $derived(!!channel && !channels.includes(channel));

    function onChannelChange(ev: Event) {
        channel = (ev.target as HTMLSelectElement).value;
        save();
    }

    function onModeChange(ev: Event) {
        mode = (ev.target as HTMLInputElement).value as VolumeControlMode;
        save();
    }

    function onValueChange(ev: Event) {
        const value = parseInt((ev.target as HTMLInputElement).value, 10);

        if (isDial || mode === VolumeControlMode.ADJUST) {
            multiplier = value;
        } else {
            setValue = value;
        }

        save();
    }

    function save() {
        if (!channel) return;

        if (isDial) {
            onSettingsChange({channel, multiplier: multiplier ?? DEFAULT_DIAL_STEP});
            return;
        }

        switch (mode) {
            case VolumeControlMode.SET:
                onSettingsChange({channel, mode, value: setValue});
                break;
            case VolumeControlMode.ADJUST:
                onSettingsChange({channel, mode, multiplier: multiplier ?? DEFAULT_ADJUST_STEP});
                break;
            case VolumeControlMode.MUTE:
                onSettingsChange({channel, mode});
                break;
        }
    }
</script>

<div class="sdpi-item">
    <label class="sdpi-item-label" for="channelSelector">{t("pi:VolumeFrame.channel")}</label>
    <select class="sdpi-item-value" id="channelSelector" value={channel ?? ""} onchange={onChannelChange}>
        <option value="" disabled>{t("pi:VolumeFrame.placeholder")}</option>
        {#if selectedChannelMissing}
            <option value={channel} selected disabled>{t(`soundChannels:${channel}.full`, {defaultValue: channel})}</option>
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
        <input class="sdpi-item-value" id="valueField" type="range" min="1" max="5" value={multiplier ?? DEFAULT_DIAL_STEP} onchange={onValueChange}/>
    </div>
{:else if mode === VolumeControlMode.SET}
    <div class="sdpi-item">
        <label class="sdpi-item-label" for="valueField">{t("pi:VolumeFrame.volume")}</label>
        <input class="sdpi-item-value" id="valueField" type="range" min="0" max="100" value={setValue} onchange={onValueChange}/>
    </div>
{:else if mode === VolumeControlMode.ADJUST}
    <div class="sdpi-item">
        <label class="sdpi-item-label" for="valueField">{t("pi:VolumeFrame.step")}</label>
        <input class="sdpi-item-value" id="valueField" type="range" min="-25" max="25" value={multiplier ?? DEFAULT_ADJUST_STEP} onchange={onValueChange}/>
    </div>
{/if}

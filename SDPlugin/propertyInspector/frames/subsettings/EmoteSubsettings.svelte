<script lang="ts">
    import {EmoteLogMode} from "#/client/rpc/messages/ActionPayloads";
    import type {EmotePayload} from "#/client/rpc/messages/ActionPayloads";
    import {t} from "../../lib/i18n.svelte";


    let {payload, onPayloadChange}: {
        payload?: EmotePayload;
        onPayloadChange: (payload: EmotePayload) => void;
    } = $props();

    let logMode = $derived(payload?.logMode ?? EmoteLogMode.DEFAULT);

    function onChange(ev: Event) {
        const value = (ev.target as HTMLInputElement).value as EmoteLogMode;
        onPayloadChange({logMode: value});
    }
</script>

<div class="sdpi-item" type="radio">
    <span class="sdpi-item-label">{t("pi:EmoteSubsettings.logSettings")}</span>
    <div class="sdpi-item-value stacked-radio">
        <input type="radio" id="emoteLogDefault" name="logMode" value={EmoteLogMode.DEFAULT} checked={logMode === EmoteLogMode.DEFAULT} onchange={onChange}>
        <label for="emoteLogDefault"><span></span>{t("pi:EmoteSubsettings.default")}</label>

        <input type="radio" id="emoteLogAlways" name="logMode" value={EmoteLogMode.ALWAYS} checked={logMode === EmoteLogMode.ALWAYS} onchange={onChange}>
        <label for="emoteLogAlways"><span></span>{t("pi:EmoteSubsettings.always")}</label>

        <input type="radio" id="emoteLogNever" name="logMode" value={EmoteLogMode.NEVER} checked={logMode === EmoteLogMode.NEVER} onchange={onChange}>
        <label for="emoteLogNever"><span></span>{t("pi:EmoteSubsettings.never")}</label>
    </div>
</div>

<style>
    .stacked-radio label {
        display: block;
    }
</style>

<script lang="ts">
    import type {MacroButtonSettings} from "#/settings/types/MacroButtonSettings";
    import {t} from "../lib/i18n.svelte";


    let {settings, onSettingsChange}: {
        settings: MacroButtonSettings | undefined;
        onSettingsChange: (settings: MacroButtonSettings) => void;
    } = $props();

    let isShared = $derived((settings?.macroId ?? 0) >= 100);
    let humanNumber = $derived((settings?.macroId ?? 0) % 100);

    function onTypeChange(ev: Event) {
        const newIsShared = (ev.target as HTMLInputElement).value === "shared";
        onSettingsChange({...settings, macroId: humanNumber + (newIsShared ? 100 : 0)});
    }

    function onNumberChange(ev: Event) {
        const input = ev.target as HTMLInputElement;
        if (!input.validity.valid || input.value === "") return;

        onSettingsChange({...settings, macroId: parseInt(input.value, 10) + (isShared ? 100 : 0)});
    }
</script>

<div class="sdpi-item" type="radio">
    <span class="sdpi-item-label">{t("pi:MacroFrame.type")}</span>
    <div class="sdpi-item-value">
        <input type="radio" id="macroTypeIndiv" name="macroType" value="indiv" checked={!isShared} onchange={onTypeChange}>
        <label for="macroTypeIndiv"><span></span>{t("pi:MacroFrame.individual")}</label>

        <input type="radio" id="macroTypeShared" name="macroType" value="shared" checked={isShared} onchange={onTypeChange}>
        <label for="macroTypeShared"><span></span>{t("pi:MacroFrame.shared")}</label>
    </div>
</div>

<div class="sdpi-item">
    <label class="sdpi-item-label" for="macroNumberField">{t("pi:MacroFrame.number")}</label>
    <input class="sdpi-item-value" id="macroNumberField" type="number" min="0" max="99" value={humanNumber} onchange={onNumberChange}/>
</div>

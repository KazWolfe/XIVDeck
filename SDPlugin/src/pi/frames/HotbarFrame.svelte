<script lang="ts">
    import type {HotbarButtonSettings} from "../../settings/types/HotbarButtonSettings";
    import {t} from "../lib/i18n.svelte";


    let {settings, onSettingsChange}: {
        settings: Partial<HotbarButtonSettings>;
        onSettingsChange: (settings: Partial<HotbarButtonSettings>) => void;
    } = $props();

    const NORMAL_PET_HOTBAR = 18;
    const CROSS_PET_HOTBAR = 19;

    const isCrossHotbar = (hotbarId: number) =>
        hotbarId === CROSS_PET_HOTBAR || (hotbarId >= 10 && hotbarId !== NORMAL_PET_HOTBAR);

    function onHotbarChange(ev: Event) {
        const value = (ev.target as HTMLSelectElement).value;

        if (value === "") {
            onSettingsChange({...settings, hotbarId: undefined, slotId: undefined});
            return;
        }

        onSettingsChange({...settings, hotbarId: parseInt(value, 10)});
    }

    function onSlotChange(ev: Event) {
        const value = (ev.target as HTMLInputElement).value;
        if (value === "") return;

        onSettingsChange({...settings, slotId: parseInt(value, 10) - 1});
    }

    let slotMax = $derived(settings.hotbarId != null && settings.hotbarId >= 0 && isCrossHotbar(settings.hotbarId) ? 16 : 12);
    let slotDisabled = $derived(settings.hotbarId == null || settings.hotbarId < 0);
</script>

<div class="sdpi-item">
    <label class="sdpi-item-label" for="hotbarSelector">{t("pi:HotbarFrame.hotbar")}</label>
    <select class="sdpi-item-value" id="hotbarSelector" value={settings.hotbarId ?? ""} onchange={onHotbarChange}>
        <option value="" disabled>{t("pi:HotbarFrame.placeholder")}</option>
        <optgroup label={t("pi:HotbarFrame.standardGroup")}>
            {#each Array(10) as _, i}
                <option value={i}>{t("pi:HotbarFrame.standard", {id: i + 1})}</option>
            {/each}
            <option value={NORMAL_PET_HOTBAR}>{t("pi:HotbarFrame.petStandard")}</option>
        </optgroup>
        <optgroup label={t("pi:HotbarFrame.crossGroup")}>
            {#each Array(8) as _, i}
                <option value={i + 10}>{t("pi:HotbarFrame.cross", {id: i + 1})}</option>
            {/each}
            <option value={CROSS_PET_HOTBAR}>{t("pi:HotbarFrame.petCross")}</option>
        </optgroup>
    </select>
</div>

<div class="sdpi-item">
    <label class="sdpi-item-label" for="slotField">{t("pi:HotbarFrame.slot")}</label>
    <input
        class="sdpi-item-value"
        id="slotField"
        type="number"
        min="1"
        max={slotMax}
        disabled={slotDisabled}
        value={settings.slotId != null && settings.slotId >= 0 ? settings.slotId + 1 : ""}
        onchange={onSlotChange}
    />
</div>

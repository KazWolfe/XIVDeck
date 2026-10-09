<script lang="ts">
    import type {HotbarButtonSettings} from "#/settings/types/HotbarButtonSettings";
    import {t} from "../lib/i18n.svelte";


    let {settings, onSettingsChange}: {
        settings: HotbarButtonSettings | undefined;
        onSettingsChange: (settings: HotbarButtonSettings) => void;
    } = $props();

    const NORMAL_PET_HOTBAR = 18;
    const CROSS_PET_HOTBAR = 19;

    const isCrossHotbar = (hotbarId: number) =>
        hotbarId === CROSS_PET_HOTBAR || (hotbarId >= 10 && hotbarId !== NORMAL_PET_HOTBAR);

    const slotCountOf = (hotbarId: number) => isCrossHotbar(hotbarId) ? 16 : 12;

    // Selections not yet saved: the frame only saves once both a hotbar and a slot are chosen. Reset to the saved
    // settings whenever those change.
    let hotbarId = $derived(settings?.hotbarId);
    let slotId = $derived(settings?.slotId);

    function onHotbarChange(ev: Event) {
        const value = (ev.target as HTMLSelectElement).value;
        if (value === "") return;

        hotbarId = parseInt(value, 10);

        // keep the chosen slot if the new hotbar has it; otherwise the user has to pick one again.
        if (slotId != null && slotId >= slotCountOf(hotbarId)) {
            slotId = undefined;
        }

        save();
    }

    function onSlotChange(ev: Event) {
        const input = ev.target as HTMLInputElement;
        if (!input.validity.valid || input.value === "") return;

        slotId = parseInt(input.value, 10) - 1;
        save();
    }

    function save() {
        if (hotbarId == null || slotId == null) return;

        onSettingsChange({_v: 1, hotbarId, slotId});
    }

    let slotMax = $derived(hotbarId != null ? slotCountOf(hotbarId) : 12);
    let slotDisabled = $derived(hotbarId == null);
</script>

<div class="sdpi-item">
    <label class="sdpi-item-label" for="hotbarSelector">{t("pi:HotbarFrame.hotbar")}</label>
    <select class="sdpi-item-value" id="hotbarSelector" value={hotbarId ?? ""} onchange={onHotbarChange}>
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
        value={slotId != null ? slotId + 1 : ""}
        onchange={onSlotChange}
    />
</div>

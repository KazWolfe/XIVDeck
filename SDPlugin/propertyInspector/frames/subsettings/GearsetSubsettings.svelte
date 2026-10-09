<script lang="ts">
    import type {GearsetPayload} from "#/client/rpc/messages/ActionPayloads";
    import {t} from "../../lib/i18n.svelte";


    let {payload, onPayloadChange}: {
        payload?: GearsetPayload;
        onPayloadChange: (payload: GearsetPayload) => void;
    } = $props();

    let useGlamourPlate = $derived(payload?.glamourPlateId != null);

    function onToggle(ev: Event) {
        useGlamourPlate = (ev.target as HTMLInputElement).checked;

        if (!useGlamourPlate) {
            onPayloadChange({});
        }
    }

    function onPlateIdChange(ev: Event) {
        const input = ev.target as HTMLInputElement;
        if (!input.validity.valid || input.value === "") return;

        onPayloadChange({glamourPlateId: parseInt(input.value, 10)});
    }
</script>

<div class="sdpi-item" type="checkbox">
    <span class="sdpi-item-label empty"></span>
    <div class="sdpi-item-value">
        <input type="checkbox" id="useGlamourPlate" checked={useGlamourPlate} onchange={onToggle}>
        <label for="useGlamourPlate"><span></span>{t("pi:GearsetSubsettings.overridePlate")}</label>
    </div>
</div>

{#if useGlamourPlate}
    <div class="sdpi-item">
        <label class="sdpi-item-label" for="plateIdField">{t("pi:GearsetSubsettings.plateId")}</label>
        <input class="sdpi-item-value" id="plateIdField" type="number" min="1" max="20" value={payload?.glamourPlateId ?? ""} onchange={onPlateIdChange}/>
    </div>
{/if}

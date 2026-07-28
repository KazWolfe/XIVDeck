<script lang="ts">
    import type {ExecActionSettings} from "../../settings/types/ExecActionSettings";
    import type {ActionEntry} from "../../rpc/messages/Action";
    import type {PiClient} from "../lib/piClient";
    import {createRpcList} from "../lib/rpcList.svelte";
    import {StringUtils} from "../../util/StringUtils";
    import {t} from "../lib/i18n.svelte";
    import EmoteSubsettings from "./subsettings/EmoteSubsettings.svelte";
    import GearsetSubsettings from "./subsettings/GearsetSubsettings.svelte";
    import type {EmotePayload, GearsetPayload} from "../../rpc/messages/ActionPayloads";


    let {settings, onSettingsChange, piClient}: {
        settings: Partial<ExecActionSettings>;
        onSettingsChange: (settings: Partial<ExecActionSettings>) => void;
        piClient: PiClient;
    } = $props();

    // svelte-ignore state_referenced_locally -- piClient is fixed for the component's lifetime.
    const actionList = createRpcList<Record<string, ActionEntry[]>>(piClient, "listActions", {});
    let actionsByType = $derived(actionList.value);

    function onPayloadChange(payload: EmotePayload | GearsetPayload): void {
        onSettingsChange({...settings, payload});
    }

    let actionsForType = $derived(settings.actionType ? (actionsByType[settings.actionType] ?? []) : []);

    const sortKey = (action: ActionEntry) => action.sortOrder ?? action.id;

    let groupedActions = $derived.by(() => {
        const sorted = [...actionsForType].sort((a, b) => sortKey(a) - sortKey(b) || a.id - b.id);

        const uncategorized = sorted.filter(a => !a.category);
        const categories = new Map<string, ActionEntry[]>();
        for (const action of sorted) {
            if (!action.category) continue;

            const group = categories.get(action.category);
            if (group) group.push(action);
            else categories.set(action.category, [action]);
        }

        return {uncategorized, categories: [...categories]};
    });

    let selectedTypeMissing = $derived(!!settings.actionType && !(settings.actionType in actionsByType));
    let selectedActionMissing = $derived(
        settings.actionId != null && !actionsForType.some(a => a.id === settings.actionId),
    );

    function actionLabel(action: ActionEntry): string {
        return action.name ? StringUtils.toTitleCase(action.name) : `#${action.id}`;
    }

    function onTypeChange(ev: Event) {
        const actionType = (ev.target as HTMLSelectElement).value;
        if (actionType === "") return;

        onSettingsChange({...settings, actionType, actionId: undefined, payload: undefined, cache: undefined});
    }

    function onActionChange(ev: Event) {
        const value = (ev.target as HTMLSelectElement).value;
        if (value === "" || !settings.actionType) return;

        const actionId = parseInt(value, 10);
        const cache = actionsForType.find(a => a.id === actionId);

        onSettingsChange({...settings, actionId, cache});
    }
</script>

<div class="sdpi-item">
    <label class="sdpi-item-label" for="typeSelector">{t("pi:ExecActionFrame.type")}</label>
    <select class="sdpi-item-value" id="typeSelector" value={settings.actionType ?? ""} onchange={onTypeChange}>
        <option value="" disabled>{t("pi:ExecActionFrame.typePlaceholder")}</option>
        {#if selectedTypeMissing}
            <option value={settings.actionType} selected disabled>{t(`actionTypes:${settings.actionType}`)}</option>
        {/if}
        {#each Object.keys(actionsByType) as type (type)}
            <option value={type}>{t(`actionTypes:${type}`)}</option>
        {/each}
    </select>
</div>

<div class="sdpi-item">
    <label class="sdpi-item-label" for="actionSelector">{t("pi:ExecActionFrame.action")}</label>
    <select class="sdpi-item-value" id="actionSelector" value={settings.actionId ?? ""} onchange={onActionChange} disabled={!settings.actionType}>
        <option value="" disabled>{t("pi:ExecActionFrame.actionPlaceholder")}</option>
        {#if selectedActionMissing}
            <option value={settings.actionId} selected disabled>{settings.cache?.name ? StringUtils.toTitleCase(settings.cache.name) : `#${settings.actionId}`}</option>
        {/if}
        {#each groupedActions.uncategorized as action (action.id)}
            <option value={action.id}>{actionLabel(action)}</option>
        {/each}
        {#each groupedActions.categories as [category, actions] (category)}
            <optgroup label={category}>
                {#each actions as action (action.id)}
                    <option value={action.id}>{actionLabel(action)}</option>
                {/each}
            </optgroup>
        {/each}
    </select>
</div>

{#if settings.actionType === "Emote"}
    <EmoteSubsettings payload={settings.payload as EmotePayload} {onPayloadChange}/>
{:else if settings.actionType === "GearSet"}
    <GearsetSubsettings payload={settings.payload as GearsetPayload} {onPayloadChange}/>
{/if}

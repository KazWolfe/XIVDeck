<script lang="ts">
    import type {ExecActionSettings} from "#/settings/types/ExecActionSettings";
    import type {ActionEntry} from "#/client/rpc/messages/Action";
    import type {PiClient} from "../lib/piClient";
    import {createRpcList} from "../lib/rpcList.svelte";
    import {StringUtils} from "#/util/StringUtils";
    import {t} from "../lib/i18n.svelte";
    import EmoteSubsettings from "./subsettings/EmoteSubsettings.svelte";
    import GearsetSubsettings from "./subsettings/GearsetSubsettings.svelte";
    import type {EmotePayload, GearsetPayload} from "#/client/rpc/messages/ActionPayloads";


    let {settings, onSettingsChange, piClient}: {
        settings: ExecActionSettings | undefined;
        onSettingsChange: (settings: ExecActionSettings) => void;
        piClient: PiClient;
    } = $props();

    // svelte-ignore state_referenced_locally -- piClient is fixed for the component's lifetime.
    const actionList = createRpcList<Record<string, ActionEntry[]>>(piClient, "listActions", {});
    let actionsByType = $derived(actionList.value);

    // The type picked but not yet saved: the frame only saves once an action of that type is chosen. Resets to the
    // saved type whenever the settings change.
    let actionType = $derived(settings?.actionType);

    // The saved action and payload belong to the saved type only.
    let isSavedType = $derived(settings != null && actionType === settings.actionType);
    let selectedActionId = $derived(isSavedType ? settings?.actionId : undefined);

    function onPayloadChange(payload: EmotePayload | GearsetPayload): void {
        if (!settings) return;

        onSettingsChange({...settings, payload});
    }

    let actionsForType = $derived(actionType ? (actionsByType[actionType] ?? []) : []);

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

    let selectedTypeMissing = $derived(!!actionType && !(actionType in actionsByType));
    let selectedActionMissing = $derived(
        selectedActionId != null && !actionsForType.some(a => a.id === selectedActionId),
    );

    function actionLabel(action: ActionEntry): string {
        return action.name ? StringUtils.toTitleCase(action.name) : `#${action.id}`;
    }

    function onTypeChange(ev: Event) {
        const value = (ev.target as HTMLSelectElement).value;
        if (value === "") return;

        actionType = value;
    }

    function onActionChange(ev: Event) {
        const value = (ev.target as HTMLSelectElement).value;
        if (value === "" || !actionType) return;

        const actionId = parseInt(value, 10);

        onSettingsChange({
            _v: 1,
            actionType,
            actionId,
            // payloads are type-specific, so a new type starts without one.
            payload: isSavedType ? settings?.payload : undefined,
            cache: actionsForType.find(a => a.id === actionId),
        });
    }
</script>

<div class="sdpi-item">
    <label class="sdpi-item-label" for="typeSelector">{t("pi:ExecActionFrame.type")}</label>
    <select class="sdpi-item-value" id="typeSelector" value={actionType ?? ""} onchange={onTypeChange}>
        <option value="" disabled>{t("pi:ExecActionFrame.typePlaceholder")}</option>
        {#if selectedTypeMissing}
            <option value={actionType} selected disabled>{t(`actionTypes:${actionType}`)}</option>
        {/if}
        {#each Object.keys(actionsByType) as type (type)}
            <option value={type}>{t(`actionTypes:${type}`)}</option>
        {/each}
    </select>
</div>

<div class="sdpi-item">
    <label class="sdpi-item-label" for="actionSelector">{t("pi:ExecActionFrame.action")}</label>
    <select class="sdpi-item-value" id="actionSelector" value={selectedActionId ?? ""} onchange={onActionChange} disabled={!actionType}>
        <option value="" disabled>{t("pi:ExecActionFrame.actionPlaceholder")}</option>
        {#if selectedActionMissing}
            <option value={selectedActionId} selected disabled>{settings?.cache?.name ? StringUtils.toTitleCase(settings.cache.name) : `#${selectedActionId}`}</option>
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

{#if isSavedType && actionType === "Emote"}
    <EmoteSubsettings payload={settings?.payload as EmotePayload} {onPayloadChange}/>
{:else if isSavedType && actionType === "GearSet"}
    <GearsetSubsettings payload={settings?.payload as GearsetPayload} {onPayloadChange}/>
{/if}

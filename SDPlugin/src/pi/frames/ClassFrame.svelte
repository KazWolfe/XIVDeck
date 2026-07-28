<script lang="ts">
    import type {ClassButtonSettings} from "../../settings/types/ClassButtonSettings";
    import type {SerializableGameClass} from "../../rpc/messages/ClassJob";
    import type {PiClient} from "../lib/piClient";
    import {createRpcList} from "../lib/rpcList.svelte";
    import {StringUtils} from "../../util/StringUtils";
    import {t} from "../lib/i18n.svelte";


    let {settings, onSettingsChange, piClient}: {
        settings: Partial<ClassButtonSettings>;
        onSettingsChange: (settings: Partial<ClassButtonSettings>) => void;
        piClient: PiClient;
    } = $props();

    // svelte-ignore state_referenced_locally -- piClient is fixed for the component's lifetime.
    const classList = createRpcList<SerializableGameClass[]>(piClient, "listClasses", []);
    let classes = $derived([...classList.value].sort((a, b) => a.sortOrder - b.sortOrder));

    let grouped = $derived.by(() => {
        const groups = new Map<string, SerializableGameClass[]>();
        for (const cls of classes) {
            if (!groups.has(cls.categoryName)) groups.set(cls.categoryName, []);
            groups.get(cls.categoryName)!.push(cls);
        }
        return groups;
    });

    let selectedMissing = $derived(settings.classId != null && !classes.some(c => c.id === settings.classId));

    function onChange(ev: Event) {
        const value = (ev.target as HTMLSelectElement).value;
        if (value === "") return;

        onSettingsChange({...settings, classId: parseInt(value, 10)});
    }
</script>

<div class="sdpi-item">
    <label class="sdpi-item-label" for="classSelector">{t("pi:ClassFrame.class")}</label>
    <select class="sdpi-item-value" id="classSelector" value={settings.classId ?? ""} onchange={onChange}>
        <option value="" disabled>{t("pi:ClassFrame.placeholder")}</option>
        {#if selectedMissing}
            <option value={settings.classId} selected disabled>{settings.cache?.name ? StringUtils.toTitleCase(settings.cache.name) : `#${settings.classId}`}</option>
        {/if}
        {#each grouped as [category, items] (category)}
            <optgroup label={category}>
                {#each items as cls (cls.id)}
                    <option value={cls.id}>{StringUtils.toTitleCase(cls.name)}</option>
                {/each}
            </optgroup>
        {/each}
    </select>
</div>

<details class="sdpi-accordion">
    <summary>{t("pi:ClassFrame.help.title")}</summary>
    <p>{t("pi:ClassFrame.help.body")}</p>
</details>

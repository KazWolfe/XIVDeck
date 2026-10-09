<script lang="ts">
    import type {CommandButtonSettings} from "#/settings/types/CommandButtonSettings";
    import {t} from "../lib/i18n.svelte";


    let {settings, onSettingsChange}: {
        settings: CommandButtonSettings | undefined;
        onSettingsChange: (settings: CommandButtonSettings) => void;
    } = $props();

    function onInput(ev: Event) {
        let value = (ev.target as HTMLTextAreaElement).value;

        if (!value.startsWith("/")) value = `/${value}`;
        value = value.replace(/\n/g, " ");

        (ev.target as HTMLTextAreaElement).value = value;

        // don't save an empty command
        if (value === "/") return;

        onSettingsChange({...settings, command: value});
    }
</script>

<div class="sdpi-item">
    <label class="sdpi-item-label" for="commandField">{t("pi:CommandFrame.command")}</label>
    <textarea class="sdpi-item-value" id="commandField" value={settings?.command ?? "/"} oninput={onInput}></textarea>
</div>

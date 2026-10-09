<script lang="ts">
    import type {JsonObject} from "@elgato/utils";
    import type {StreamDeckSocket} from "../lib/StreamDeckSocket";
    import {t} from "../lib/i18n.svelte";
    import Banner from "./Banner.svelte";
    import ExternalLink from "../components/ExternalLink.svelte";
    import RichText from "../components/RichText.svelte";


    let {socket}: { socket: StreamDeckSocket<JsonObject> } = $props();

    const LINKS: Record<string, string> = {
        launcher: "https://goatcorp.github.io",
        discord: "https://discord.gg/holdshift",
    };
</script>

{#snippet link(name: string, content: string)}
    <ExternalLink {socket} href={LINKS[name]}>{content}</ExternalLink>
{/snippet}

<Banner title={t("pi:GameDisconnectedBanner.title")}>
    <p><RichText text={t("pi:GameDisconnectedBanner.body")} tag={link}/></p>
    <p><RichText text={t("pi:GameDisconnectedBanner.help")} tag={link}/></p>
</Banner>

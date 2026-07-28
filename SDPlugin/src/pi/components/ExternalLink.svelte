<script lang="ts">
    import type {Snippet} from "svelte";
    import type {JsonObject} from "@elgato/utils";
    import type {StreamDeckSocket} from "../lib/StreamDeckSocket";

    let {socket, href, children}: { socket: StreamDeckSocket<JsonObject>; href: string; children: Snippet } = $props();

    function open(ev: MouseEvent): void {
        ev.preventDefault();
        socket.openUrl(href);
    }
</script>

<a {href} onclick={open}>{@render children()}</a>

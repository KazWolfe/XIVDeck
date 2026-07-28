<script lang="ts">
    import type {Snippet} from "svelte";

    let {title, children}: { title: string; children: Snippet } = $props();
</script>

<div class="connection-banner">
    <h2>{title}</h2>
    {@render children()}
</div>

<style>
    .connection-banner {
        margin: 4px 0 8px;
        padding: 10px 14px;
        border: 1px solid rgba(255, 196, 0, 0.4);
        border-radius: var(--sdpi-borderradius);
        background-color: rgba(255, 196, 0, 0.08);
        color: var(--sdpi-color);
    }

    .connection-banner h2 {
        margin: 0 0 6px;
        font-size: 10pt;
        text-align: center;
        color: #ffc400;
    }

    .connection-banner :global(p) {
        margin: 4px 0;
    }

    .connection-banner :global(a) {
        color: inherit;
        text-decoration: underline;
        cursor: pointer;
    }
</style>

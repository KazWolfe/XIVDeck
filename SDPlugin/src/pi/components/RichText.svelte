<script lang="ts">
    import type {Snippet} from "svelte";

    /**
     * Renders a translated string whose markup is limited to `<name>text</name>` tags, handing each one to `tag` so the
     * caller decides what it becomes (a link, emphasis, ...). Everything else renders as plain text.
     */
    let {text, tag}: { text: string; tag: Snippet<[name: string, content: string]> } = $props();

    type Part = { text: string } | { name: string; content: string };

    let parts = $derived.by(() => {
        const result: Part[] = [];
        const pattern = /<(\w+)>(.*?)<\/\1>/g;

        let last = 0;
        for (const match of text.matchAll(pattern)) {
            if (match.index > last) result.push({text: text.slice(last, match.index)});
            result.push({name: match[1], content: match[2]});
            last = match.index + match[0].length;
        }
        if (last < text.length) result.push({text: text.slice(last)});

        return result;
    });
</script>

{#each parts as part, i (i)}{#if "name" in part}{@render tag(part.name, part.content)}{:else}{part.text}{/if}{/each}

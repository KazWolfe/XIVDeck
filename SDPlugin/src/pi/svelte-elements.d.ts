import "svelte/elements";

declare module "svelte/elements" {
    interface HTMLAttributes<T> {
        // sdpi.css lays out `<div class="sdpi-item" type="radio">` (and friends) by this attribute.
        type?: string | undefined | null;
    }
}

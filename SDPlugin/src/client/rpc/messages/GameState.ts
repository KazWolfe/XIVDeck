export interface FocusState {
    isFocused: boolean;

    /** unix timestamp when the game was last (known to be) focused, or `null` if never. */
    lastFocusTime?: number | null;
}

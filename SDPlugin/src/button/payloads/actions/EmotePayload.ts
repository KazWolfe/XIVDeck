
export type EmotePayload = {
    /**
     * Set if a log message should be sent for this emote action.
     * - `true`: Always send a log message when this button is used.
     * - `null` (default): Defer to the current setting when this button is used.
     * - `false`: Never send a log message when this button is used.d
     */
    logMode?: null | boolean;
}
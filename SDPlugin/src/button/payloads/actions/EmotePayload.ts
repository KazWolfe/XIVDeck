
export enum EmoteLogMode {
    DEFAULT = "default",
    ALWAYS = "always",
    NEVER = "never"
}

export type EmotePayload = {
    logMode?: EmoteLogMode;
}
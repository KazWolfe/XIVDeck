import i18next from "i18next";

const modules = import.meta.glob<{ default: object }>("../../../assets/locales/*/*.json", {eager: true});

const resources: Record<string, Record<string, object>> = {};

for (const [path, mod] of Object.entries(modules)) {
    const match = /locales\/([^/]+)\/([^/]+)\.json$/.exec(path);
    if (!match) continue;

    const [, lang, namespace] = match;
    (resources[lang] ??= {})[namespace] = mod.default;
}

await i18next.init({
    resources,
    fallbackLng: "en",
    defaultNS: "pi",
    interpolation: {escapeValue: false},
});

let languageTick = $state(0);

export function setLanguage(lang: string): void {
    void i18next.changeLanguage(lang).then(() => languageTick++);
}

export function t(key: string, options?: Record<string, unknown>): string {
    languageTick;
    return i18next.t(key, options) as string;
}

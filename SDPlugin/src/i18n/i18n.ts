import i18next from "i18next";
import * as fs from "node:fs";
import * as path from "node:path";

const localesDir = path.join(__dirname, "locales");

function loadResources(): Record<string, Record<string, object>> {
    const resources: Record<string, Record<string, object>> = {};

    let languages: string[];
    try {
        languages = fs.readdirSync(localesDir);
    } catch (err) {
        console.warn(`[i18n] Failed to read locales directory "${localesDir}":`, err);
        return resources;
    }

    for (const lang of languages) {
        const langDir = path.join(localesDir, lang);
        if (!fs.statSync(langDir).isDirectory()) continue;

        const namespaces: Record<string, object> = {};
        for (const file of fs.readdirSync(langDir)) {
            if (!file.endsWith(".json")) continue;

            namespaces[file.slice(0, -".json".length)] = JSON.parse(fs.readFileSync(path.join(langDir, file), "utf8"));
        }

        resources[lang] = namespaces;
    }

    return resources;
}

export class I18n {
    public static async initialize(language: string): Promise<void> {
        await i18next.init({
            resources: loadResources(),
            fallbackLng: "en",
            defaultNS: "controls",
            interpolation: {escapeValue: false},
        });

        await i18next.changeLanguage(language);
    }
}

export default i18next;

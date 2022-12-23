import { TeleportButtonSettings } from "../../button/buttons/TeleportButton";
import piInstance from "../../inspector";
import { FFXIVApi } from "../../link/ffxivplugin/FFXIVApi";
import { Aetheryte } from "../../link/ffxivplugin/GameTypes";
import { PIUtils } from "../../util/PIUtils";
import { BaseFrame } from "../BaseFrame";

export class TeleportFrame extends BaseFrame<TeleportButtonSettings> {

    // settings
    settings?: TeleportButtonSettings;

    // cache: WorldArea -> [Region -] Territory -> Aetheryte
    aetheryteCache: Map<string, Map<string, Aetheryte[]>> = new Map<string, Map<string, Aetheryte[]>>();
    aetheryteIdCache: Map<string, Aetheryte> = new Map<string, Aetheryte>();

    // temp state
    selectedWorldArea?: string;
    selectedAetheryte?: string;

    // dom
    worldAreaSelector: HTMLSelectElement;
    aetheryteSelector: HTMLSelectElement;

    constructor() {
        super();

        this.worldAreaSelector = document.createElement("select");
        this.worldAreaSelector.id = "regionSelector";
        this.worldAreaSelector.onchange = this._onRegionChange.bind(this);

        this.aetheryteSelector = document.createElement("select");
        this.aetheryteSelector.id = "aetheryteSelector";
        this.aetheryteSelector.onchange = this._onAetheryteChange.bind(this);

        piInstance.xivPluginLink.on("_ready", this.loadGameData.bind(this));
    }

    loadSettings(settings: TeleportButtonSettings | undefined): void {
        this.settings = settings;

        this.selectedWorldArea = this.settings?.cache?.worldArea;

        if (this.settings?.aetheryteId != null && this.settings?.subId != null) {
            this.selectedAetheryte = `${this.settings?.aetheryteId}.${this.settings?.subId}`;
        }
    }

    renderHTML(): void {
        this.domParent.append(PIUtils.createPILabeledElement("Region", this.worldAreaSelector));
        this.domParent.append(PIUtils.createPILabeledElement("Aetheryte", this.aetheryteSelector));
    }

    async loadGameData(): Promise<void> {
        let results = await FFXIVApi.Teleport.getAetherytes();

        results.forEach((entry) => {
            let regionMapping = this.aetheryteCache.get(entry.worldArea) ?? new Map<string, Aetheryte[]>();
            let areaList = regionMapping.get(entry.territory) ?? new Array<Aetheryte>();
            areaList.push(entry);
            regionMapping.set(entry.territory, areaList);
            this.aetheryteCache.set(entry.worldArea, regionMapping);

            this.aetheryteIdCache.set(`${entry.id}.${entry.subId}`, entry);
        });

        this._loadRegionDropdown();
        this._loadAetheryteDropdown();
    }

    private _loadRegionDropdown() {
        this.worldAreaSelector.length = 0;

        let placeholder = PIUtils.createDefaultSelection("Select region...");
        this.worldAreaSelector.add(placeholder);

        if (this.selectedWorldArea == null) {
            placeholder.selected = true;
        } else if (!this.aetheryteCache.has(this.selectedWorldArea)) {
            let failsafe = PIUtils.createDefaultSelection(this.selectedWorldArea);
            failsafe.selected = true;

            this.worldAreaSelector.add(failsafe);
        }

        this.aetheryteCache.forEach((_, k) => {
            let element = document.createElement("option");
            element.value = k;
            element.textContent = k;

            if (k == this.selectedWorldArea) {
                element.selected = true;
            }

            this.worldAreaSelector.add(element);
        });
    }

    private _loadAetheryteDropdown(): void {
        this.aetheryteSelector.innerHTML = "";

        let placeholder = PIUtils.createDefaultSelection("Select aetheryte...");
        this.aetheryteSelector.add(placeholder);
        
        if (this.selectedWorldArea == null || this.selectedAetheryte == null) {
            placeholder.selected = true;
            if (this.selectedWorldArea == null) return;
        }

        let regionAetherytes = this.aetheryteCache.get(this.selectedWorldArea) ?? new Map<string, Aetheryte[]>();

        var cache = this.settings?.cache;
        if (cache != null && cache.worldArea == this.selectedWorldArea && !regionAetherytes.has(cache.territory)) {
            let optGroup = document.createElement("optgroup");
            optGroup.id = cache.territory;
            optGroup.label = cache.territory;
            optGroup.disabled = true;

            let option = document.createElement("option");
            option.value = `${this.selectedAetheryte}`;
            option.textContent = cache.name;
            option.disabled = true;
            option.selected = true;

            optGroup.append(option);
            this.aetheryteSelector.append(optGroup);
        }

        regionAetherytes.forEach((v, k) => {
            let optGroup = document.createElement("optgroup");
            optGroup.id = k;
            optGroup.label = k;

            let seenValues = new Array<string>();

            v.forEach((entry) => {
                let option = document.createElement("option");
                option.value = `${entry.id}.${entry.subId}`;
                option.innerText = entry.name;

                if (option.value == this.selectedAetheryte) {
                    option.selected = true;
                }

                seenValues.push(option.value);
                optGroup.append(option);
            });
            
            // handle case where the target aetheryte wasn't loaded
            if (cache?.territory == k && this.selectedAetheryte != null && !seenValues.includes(this.selectedAetheryte)) {
                let failsafe = document.createElement("option");
                failsafe.innerText = cache.name;
                failsafe.selected = true;
                failsafe.disabled = true;
                
                optGroup.prepend(failsafe);
                
                console.log("adding failsafe", this, cache, seenValues);
            }

            this.aetheryteSelector.append(optGroup);
        });
    }

    private _onRegionChange(): void {
        this.selectedWorldArea = this.worldAreaSelector.value;
        this._loadAetheryteDropdown();
    }

    private _onAetheryteChange(): void {
        this.selectedAetheryte = this.aetheryteSelector.value;
        let split = this.aetheryteSelector.value.split(".", 2);

        console.log("switched to aetheryte", split);

        this.setSettings({
            aetheryteId: parseInt(split[0]),
            subId: parseInt(split[1]) ?? 0,
            cache: this.aetheryteIdCache.get(this.selectedAetheryte)
        });
    }
}
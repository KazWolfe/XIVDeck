import { fail } from "assert";
import { optimize } from "webpack";
import { TeleportButtonSettings } from "../../button/buttons/TeleportButton";
import i18n from "../../i18n/i18n";
import piInstance from "../../inspector";
import { FFXIVApi } from "../../link/ffxivplugin/FFXIVApi";
import { Aetheryte } from "../../link/ffxivplugin/GameTypes";
import { PIUtils } from "../../util/PIUtils";
import { BaseFrame } from "../BaseFrame";

export class TeleportFrame extends BaseFrame<TeleportButtonSettings> {
    
    // settings
    settings?: TeleportButtonSettings;
    
    // cache: Region -> Locale -> Aetheryte
    aetheryteCache: Map<string, Map<string, Aetheryte[]>> = new Map<string, Map<string, Aetheryte[]>>();
    
    // temp state
    selectedRegion?: string;
    selectedAetheryte?: string;
    
    // dom
    regionSelector: HTMLSelectElement;
    aetheryteSelector: HTMLSelectElement;
    
    constructor() {
        super();

        this.regionSelector = document.createElement("select");
        this.regionSelector.id = "regionSelector";
        this.regionSelector.onchange = this._onRegionChange.bind(this);
        
        this.aetheryteSelector = document.createElement("select");

        piInstance.xivPluginLink.on("_ready", this.loadGameData.bind(this));
    }
    
    loadSettings(settings: TeleportButtonSettings | undefined): void {
        this.settings = settings;
        
        this.selectedRegion = this.settings?.cache?.region;
        this.selectedAetheryte = `${this.settings?.aetheryteId}.${this.settings?.subId}`;
    }

    renderHTML(): void {
        this.domParent.append(PIUtils.createPILabeledElement("Region", this.regionSelector));
        this.domParent.append(PIUtils.createPILabeledElement("Aetheryte", this.aetheryteSelector));
    }
    
    async loadGameData(): Promise<void> {
        let results = await FFXIVApi.Teleport.getAetherytes();
        
        results.forEach((entry) => {
            let regionMapping = this.aetheryteCache.get(entry.region) ?? new Map<string, Aetheryte[]>();
            let areaList = regionMapping.get(entry.territory) ?? new Array<Aetheryte>();
            areaList.push(entry);
            regionMapping.set(entry.territory, areaList);
            this.aetheryteCache.set(entry.region, regionMapping);
        });
        
        this._loadRegionDropdown();
        this._loadAetheryteDropdown();
    }
    
    private _loadRegionDropdown() {
        this.regionSelector.length = 0;
        
        if (this.selectedRegion == null) {
            let placeholder = PIUtils.createDefaultSelection("Select region...");
            placeholder.selected = true;
            
            this.regionSelector.add(placeholder);
        } else if (!this.aetheryteCache.has(this.selectedRegion)) {
            let failsafe = PIUtils.createDefaultSelection(this.selectedRegion);
            failsafe.selected = true;
            
            this.regionSelector.add(failsafe);
        }
        
        this.aetheryteCache.forEach((_, k) => {
            let element = document.createElement("option");
            element.id = k;
            element.textContent = k;
            
            if (k == this.selectedRegion) {
                element.selected = true;
            }
            
            this.regionSelector.add(element);
        })
    }
    
    private _loadAetheryteDropdown(): void {
        this.aetheryteSelector.innerHTML = "";
        
        if (this.selectedRegion == null || this.selectedAetheryte == null) {
            let placeholder = PIUtils.createDefaultSelection("Select aetheryte...");
            placeholder.selected = true;
            
            this.aetheryteSelector.add(placeholder);
            return;
        }
        
        let regionAetherytes = this.aetheryteCache.get(this.selectedRegion) ?? new Map<string, Aetheryte[]>();
        
        var cache = this.settings?.cache;
        if (cache != null && !regionAetherytes.has(cache.territory)) {
            let optGroup = document.createElement("optgroup");
            optGroup.id = cache.territory;
            optGroup.label = cache.territory;
            
            let option = document.createElement("option");
            option.id = `${this.selectedAetheryte}`;
            option.textContent = cache.name;
            
            optGroup.append(option);
            this.aetheryteSelector.prepend(optGroup);
        }
        
        regionAetherytes.forEach((v, k) => {
            let optGroup = document.createElement("optgroup");
            optGroup.id = k;
            optGroup.label = k;
            
            v.forEach((entry) => {
                let option = document.createElement("option");
                option.id = `${entry.id}.${entry.subId}`;
                option.innerText = entry.name;
                
                if (option.id == this.selectedAetheryte) {
                    option.selected = true;
                }
                
                optGroup.append(option);
            });
            
            this.aetheryteSelector.append(optGroup);
        })
    }
    
    private _onRegionChange(): void {
        this.selectedRegion = this.regionSelector.value;
        this._loadAetheryteDropdown();
    }
}
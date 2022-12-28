import {BaseFrame} from "../BaseFrame";
import {ClassButtonSettings} from "../../button/buttons/ClassButton";
import piInstance from "../../inspector";
import {StringUtils} from "../../util/StringUtils";
import {PIUtils} from "../../util/PIUtils";
import {FFXIVApi} from "../../link/ffxivplugin/FFXIVApi";
import i18n from "../../i18n/i18n";

export class ClassFrame extends BaseFrame<ClassButtonSettings> {
    classSelector: HTMLSelectElement;
    selected: number = -1; // prevent against race to load in visible settings
    selectedName: string = i18n.t("frames:class.unknown");
    
    constructor() {
        super();
        
        this.classSelector = document.createElement("select")
        this.classSelector.id = "classSelector";
        
        piInstance.xivPluginLink.on("_ready", this.populateClasses.bind(this))
    }
    
    loadSettings(settings: ClassButtonSettings): void {
        this.selected = settings.classId || this.selected;
        this.selectedName = settings.className || this.selectedName;
        
        if (this.selected >= 0) {
            this._preloadDropdown();
        }
    }

    renderHTML(): void {
        let sdItem = PIUtils.createPILabeledElement(i18n.t("frames:class.class"), this.classSelector);
        
        this.classSelector.options.length = 0;
        this.classSelector.add(PIUtils.createDefaultSelection(i18n.t("frames:class.default")));
        this.classSelector.onchange = this._onClassUpdate.bind(this);

        this.domParent.append(sdItem);
    }
    
    async populateClasses(): Promise<void> {
        let groupCache: Map<string, HTMLOptGroupElement> = new Map<string, HTMLOptGroupElement>()
        let classData = await FFXIVApi.GameClass.getClasses(true);
        
        // sort this list properly
        classData = classData.sort((a, b) => (a.sortOrder > b.sortOrder) ? 1 : -1);
        
        classData.forEach((cl) => {
            let group = groupCache.get(cl.categoryName);
            if (group == null) {
                group = document.createElement("optgroup");
                group.label = cl.categoryName;
                groupCache.set(cl.categoryName, group);
            }
            
            let selection: HTMLOptionElement = document.createElement("option");
            selection.value = cl.id.toString();
            selection.innerText = StringUtils.toTitleCase(cl.name);
            
            group.append(selection);
        });

        this.classSelector.options.length = 0;
        this.classSelector.add(PIUtils.createDefaultSelection(i18n.t("frames:class.default")));
        groupCache.forEach((v) => {
            this.classSelector.add(v);
        })
        
        // handle the edge case of a class being assigned to the button, but not returned by the game
        if (this.selected >= 0 && classData.map(cl => cl.id).indexOf(this.selected) < 0) {
            this._preloadDropdown(groupCache);
        }

        this.classSelector.value = (this.selected >= 0) ? this.selected.toString() : "default";
    }
    
    private _preloadDropdown(groupCache?: Map<string, HTMLOptGroupElement>): void {
        let parent: HTMLOptGroupElement | HTMLSelectElement = this.classSelector;
        
        FFXIVApi.GameClass.getClass(this.selected).then(cl => {
            this.selectedName = cl.name;
            
            if (groupCache == null) {
                groupCache = new Map<string, HTMLOptGroupElement>();
            }

            let pc = groupCache.get(cl.categoryName); 
            if (pc == null) {
                pc = document.createElement("optgroup");
                pc.label = cl.categoryName;
                parent.append(pc);
            }
            
            parent = pc;
        }).catch(() => { }).finally(() => {
            let selection: HTMLOptionElement = document.createElement("option");
            selection.value = this.selected.toString();
            selection.selected = true;
            selection.disabled = true;
            selection.innerText = StringUtils.toTitleCase(this.selectedName);

            parent.append(selection);
        }) 
    }
    
    private _onClassUpdate(_: Event): void {
        let selected = this.classSelector.value;
        
        if (selected === "default") {
            console.warn("Default value was somehow selected, aborting update.")
            return
        }
        
        this.selected = parseInt(selected);
        this.selectedName = this.classSelector.selectedOptions[0].text;
        
        this.setSettings({
            classId: this.selected,
            className: this.selectedName
        });
    }
}
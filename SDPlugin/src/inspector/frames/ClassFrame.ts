import {BaseFrame} from "../BaseFrame";
import {ClassButtonSettings} from "../../button/buttons/ClassButton";
import piInstance from "../../inspector";
import {StringUtils} from "../../util/StringUtils";
import {PIUtils} from "../../util/PIUtils";
import {FFXIVApi} from "../../link/ffxivplugin/FFXIVApi";

export class ClassFrame extends BaseFrame<ClassButtonSettings> {
    classSelector: HTMLSelectElement;
    selected: string = "default"; // prevent against race to load in visible settings
    
    constructor() {
        super();
        
        this.classSelector = document.createElement("select")
        this.classSelector.id = "classSelector";
        
        piInstance.xivPluginLink.on("initReply", this.populateClasses.bind(this))
    }
    
    loadSettings(settings: ClassButtonSettings): void {
        this.classSelector.value = this.selected;
        this.selected = settings.classId.toString();
    }

    renderHTML(): void {
        let sdItem = PIUtils.createPILabeledElement("Class", this.classSelector);
        
        this.classSelector.options.length = 0;
        this.classSelector.add(PIUtils.createDefaultSelection("class"));
        this.classSelector.onchange = this._onClassUpdate.bind(this);

        this.domParent.append(sdItem);
        this.populateClasses().then(_ => {});
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
        this.classSelector.add(PIUtils.createDefaultSelection("class"));
        console.log(this, groupCache);
        groupCache.forEach((v) => {
            this.classSelector.add(v);
        })

        this.classSelector.value = this.selected;
    }
    
    private _onClassUpdate(_: Event): void {
        let selected = this.classSelector.value;
        
        if (selected === "default") {
            console.warn("Default value was somehow selected, aborting update.")
            return
        }
        
        this.selected = selected;
        
        this.setSettings({
            classId: parseInt(selected),
        });
    }
}
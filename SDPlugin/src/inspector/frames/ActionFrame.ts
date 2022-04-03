import {BaseFrame} from "../BaseFrame";
import {ActionButtonSettings} from "../../button/buttons/ActionButton";
import {FFXIVAction} from "../../link/ffxivplugin/GameTypes";
import piInstance from "../../inspector";
import {PIUtils} from "../../util/PIUtils";
import {StringUtils} from "../../util/StringUtils";
import {FFXIVApi} from "../../link/ffxivplugin/FFXIVApi";

const NAME_SUBSTITUTIONS: Record<string, string> = {
    "FieldMarker": "Waymark",
};


export class ActionFrame extends BaseFrame<ActionButtonSettings> {
    typeSelector: HTMLSelectElement;
    actionSelector: HTMLSelectElement;
    
    actionData: Map<string, FFXIVAction[]> = new Map<string, FFXIVAction[]>();
    
    selectedType: string = "default";
    selectedAction: number = -1;

    constructor() {
        super();

        this.typeSelector = document.createElement("select");
        this.typeSelector.id = "typeSelector";
        this.typeSelector.onchange = this._onTypeChange.bind(this);

        this.actionSelector = document.createElement("select");
        this.actionSelector.id = "actionSelector";
        this.actionSelector.onchange = this._onItemChange.bind(this);
        
        this.typeSelector.add(PIUtils.createDefaultSelection("action type"));
        
        piInstance.xivPluginLink.on("_ready", this.loadGameData.bind(this));
    }

    loadSettings(settings: ActionButtonSettings): void {
        this.selectedType = settings.actionType || this.selectedType;
        this.selectedAction = (settings.actionId !== undefined) ? settings.actionId : this.selectedAction;
    }

    renderHTML(): void {
        this.domParent.append(PIUtils.createPILabeledElement("Type", this.typeSelector));
        this.domParent.append(PIUtils.createPILabeledElement("Action", this.actionSelector));
    }

    async loadGameData(): Promise<void> {
        this.actionData = await FFXIVApi.Action.getActions();
        
        this._renderTypes();
        this._renderItems();
        this._fillDropdowns();
    }
    
    private _fillDropdowns() {
        if (this.actionData.has(this.selectedType)) {
            this.typeSelector.value = this.selectedType;
            this.actionSelector.value = this.selectedAction.toString();
        }
    }
    
    private _renderTypes() {
        this.typeSelector.options.length = 0;
        this.typeSelector.add(PIUtils.createDefaultSelection("action type"));
        
        console.log(this.actionData, typeof(this.actionData))
        
        this.actionData.forEach((_, k) => {
            let option = document.createElement("option");
            option.value = k;
            option.innerText = StringUtils.expandCaps(NAME_SUBSTITUTIONS[k] || k);
            this.typeSelector.add(option);
        })
    }
    
    private _renderItems() {
        // reset this dropdown and clear out all options
        this.actionSelector.innerHTML = "";

        let groupCache: Map<string, HTMLOptGroupElement> = new Map<string, HTMLOptGroupElement>();

        if (this.selectedType == "default") {
            this.actionSelector.disabled = true;
            return
        }
        
        let items = this.actionData.get(this.selectedType) || [];
        console.debug("Loaded current selectable items for this PI instance", items)

        this.actionSelector.add(PIUtils.createDefaultSelection("action"));
        
        items.forEach((ac) => {
            let parent: HTMLSelectElement | HTMLOptGroupElement = this.actionSelector;
            
            if (ac.category) {
                if (groupCache.has(ac.category)) {
                    parent = groupCache.get(ac.category)!;
                } else {
                    parent = document.createElement("optgroup");
                    parent.label = StringUtils.toTitleCase(ac.category);
                    this.actionSelector.append(parent);
                    groupCache.set(ac.category, parent);
                }
            }
                
            let option = document.createElement("option");
            option.value = ac.id.toString();
            option.innerText = `[#${ac.id}] ${StringUtils.toTitleCase(ac.name || "unknown")}`
            
            parent.append(option);
        });

        this.actionSelector.disabled = false;
    }
    
    private _onTypeChange() {
        this.selectedType = this.typeSelector.value;
        this._renderItems();
    }
    
    private _onItemChange() {
        let newSelection = this.actionSelector.value;
        
        this.selectedType = this.typeSelector.value;
        
        if (newSelection == "default") {
            return;
        }
        
        this.selectedAction = parseInt(newSelection);
        
        this.setSettings({
            actionId: this.selectedAction,
            actionType: this.selectedType
        })
    }
}

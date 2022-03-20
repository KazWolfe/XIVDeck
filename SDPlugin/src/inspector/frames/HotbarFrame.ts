import {BaseFrame} from "../BaseFrame";
import {HotbarButtonSettings} from "../../button/buttons/HotbarButton";
import {PIUtils} from "../../util/PIUtils";

export class HotbarFrame extends BaseFrame<HotbarButtonSettings> {
    // html elements
    hotbarSelector: HTMLSelectElement;
    slotField: HTMLInputElement;
    
    // state
    hotbarId: number = -1;
    slotId: number = -1;
    
    constructor() {
        super();
        
        this.hotbarSelector = document.createElement("select");
        this.hotbarSelector.id = "hotbarSelector"
        
        this.slotField = document.createElement("input")
        this.slotField.type = "number";
        this.slotField.id = "slotField";

        // start this field disabled, will be enabled if hotbar values set
        this.slotField.disabled = true;
    }
    
    loadSettings(settings: HotbarButtonSettings): void {
        this.hotbarId = settings.hotbarId !== undefined ? settings.hotbarId : this.hotbarId;
        this.slotId = settings.slotId !== undefined ? settings.slotId : this.slotId;
        
        this.hotbarSelector.value = (this.hotbarId >= 0) ? this.hotbarId.toString() : "default";
        this._setSlotFieldParams();
        
        if (this.hotbarId >= 0) {
            this.slotField.value = (this.slotId + 1).toString();
        }
    }

    renderHTML(): void {
        this.domParent.append(PIUtils.createPILabeledElement("Hotbar", this.hotbarSelector));
        this.domParent.append(PIUtils.createPILabeledElement("Slot", this.slotField));
        
        this._renderHotbarSelector();
        this.hotbarSelector.value = (this.hotbarId >= 0) ? this.hotbarId.toString() : "default";
        
        // add event listener hooks
        this.hotbarSelector.onchange = this._onUpdate.bind(this);
        this.slotField.oninput = this._onUpdate.bind(this);
    }
    
    private _renderHotbarSelector() {
        let standardGroup = document.createElement("optgroup");
        standardGroup.label = "Standard Hotbars";
        
        let crossGroup = document.createElement("optgroup");
        crossGroup.label = "Cross Hotbars";
        
        for (let i = 0; i <= 17; i++) {
            let isCrossHotbar = (i >= 10);
            let humanIndex = isCrossHotbar ? (i - 9) : (i + 1);
            
            let entry = document.createElement("option");
            entry.value = i.toString();
            entry.innerText = `${isCrossHotbar ? "Cross ": ""} Hotbar ${humanIndex}`
            
            if (isCrossHotbar) {
                crossGroup.append(entry);
            } else {
                standardGroup.append(entry);
            }
        }
        
        console.log(standardGroup, crossGroup);
        this.hotbarSelector.add(PIUtils.createDefaultSelection("hotbar"));
        this.hotbarSelector.add(standardGroup);
        this.hotbarSelector.add(crossGroup);
    }
    
    private _setSlotFieldParams() {
        this.slotField.setAttribute("style", "");
        
        if (this.hotbarId < 0) {
            // unset or init
            this.slotField.value = "";
            this.slotField.min = "";
            this.slotField.max = "";
            this.slotField.disabled = true;
            return
        }

        let isCrossHotbar = (this.hotbarId >= 10);

        this.slotField.disabled = false;
        this.slotField.min = "1";
        
        if (isCrossHotbar) {
            this.slotField.max = "16";
        } else {
            this.slotField.max = "12";
        }
    }
    
    private _onUpdate(ev: Event) {
        let selectedHotbar = this.hotbarSelector.value;
        let selectedSlot = this.slotField.value;

        if (selectedHotbar === "default") {
            // not sure how you'd manage this state, but here we are.
            this.hotbarId = -1;
            this.slotId = -1;
            
            this._setSlotFieldParams();
            return;
        }

        // validate and tweak things based on the selected hotbar slot
        let hotbarNum = parseInt(selectedHotbar);
        let changedHotbar = hotbarNum != this.hotbarId;
            
        this.hotbarId = hotbarNum;
        this._setSlotFieldParams();
        if (!PIUtils.validateMinMaxOfNumberField(this.slotField)) {
            if (changedHotbar) {
                this.slotField.value = "";
            }
            this.slotField.setAttribute("style", "color: orangered")
            return;
        }
        
        if (selectedSlot == "") {
            return;
        }
        
        this.slotId = parseInt(selectedSlot) - 1;
        
        this.setSettings({
            hotbarId: this.hotbarId,
            slotId: this.slotId
        })
    }
}
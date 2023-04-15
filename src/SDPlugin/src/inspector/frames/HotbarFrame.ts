import {BaseFrame} from "../BaseFrame";
import {HotbarButtonSettings} from "../../button/buttons/HotbarButton";
import {PIUtils} from "../../util/PIUtils";
import i18n from "../../i18n/i18n";

export class HotbarFrame extends BaseFrame<HotbarButtonSettings> {
    // html elements
    channelSelector: HTMLSelectElement;
    slotField: HTMLInputElement;
    
    // state
    hotbarId: number = -1;
    slotId: number = -1;
    
    constructor() {
        super();
        
        this.channelSelector = document.createElement("select");
        this.channelSelector.id = "hotbarSelector"
        
        this.slotField = document.createElement("input")
        this.slotField.type = "number";
        this.slotField.id = "slotField";

        // start this field disabled, will be enabled if hotbar values set
        this.slotField.disabled = true;
    }
    
    loadSettings(settings: HotbarButtonSettings): void {
        this.hotbarId = settings.hotbarId !== undefined ? settings.hotbarId : this.hotbarId;
        this.slotId = settings.slotId !== undefined ? settings.slotId : this.slotId;
        
        this.channelSelector.value = (this.hotbarId >= 0) ? this.hotbarId.toString() : "default";
        this._setSlotFieldParams();
        
        if (this.hotbarId >= 0) {
            this.slotField.value = (this.slotId + 1).toString();
        }
    }

    renderHTML(): void {
        this.domParent.append(PIUtils.createPILabeledElement(i18n.t("frames:hotbar.hotbar"), this.channelSelector));
        this.domParent.append(PIUtils.createPILabeledElement(i18n.t("frames:hotbar.slot"), this.slotField));
        
        this._renderHotbarSelector();
        this.channelSelector.value = (this.hotbarId >= 0) ? this.hotbarId.toString() : "default";
        
        // add event listener hooks
        this.channelSelector.onchange = this._onUpdate.bind(this);
        this.slotField.oninput = this._onUpdate.bind(this);
    }
    
    private _renderHotbarSelector() {
        let standardGroup = document.createElement("optgroup");
        standardGroup.label = i18n.t("frames:hotbar.standard-pl")
        
        let crossGroup = document.createElement("optgroup");
        crossGroup.label = i18n.t("frames:hotbar.cross-pl")
        
        for (let i = 0; i <= 17; i++) {
            let isCrossHotbar = (i >= 10);
            let humanIndex = isCrossHotbar ? (i - 9) : (i + 1);
            
            let entry = document.createElement("option");
            entry.value = i.toString();
            if (isCrossHotbar) {
                entry.innerText = i18n.t("frames:hotbar.cross", {"id": humanIndex})
            } else {
                entry.innerText = i18n.t("frames:hotbar.standard", {"id": humanIndex})
            }
            
            if (isCrossHotbar) {
                crossGroup.append(entry);
            } else {
                standardGroup.append(entry);
            }
        }
        
        this.channelSelector.add(PIUtils.createDefaultSelection(i18n.t("frames:hotbar.default")));
        this.channelSelector.add(standardGroup);
        this.channelSelector.add(crossGroup);
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
        let selectedHotbar = this.channelSelector.value;
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
        if (!this.slotField.validity.valid) {
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
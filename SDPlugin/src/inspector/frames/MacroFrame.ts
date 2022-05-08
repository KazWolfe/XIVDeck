import {BaseFrame} from "../BaseFrame";
import {MacroButtonSettings} from "../../button/buttons/MacroButton";
import {PIUtils} from "../../util/PIUtils";
import i18n from "../../i18n/i18n";

export class MacroFrame extends BaseFrame<MacroButtonSettings> {
    // settings
    macroId: number = -1;
    
    // dom
    macroNumberField: HTMLInputElement;
    macroRadioField: HTMLElement;
    
    constructor() {
        super();
        
        this.macroNumberField = document.createElement("input");
        this.macroNumberField.type = "number";
        this.macroNumberField.min = "0";
        this.macroNumberField.max = "99";
        
        this.macroRadioField = PIUtils.generateRadioSelection(i18n.t("frames:macro.type"), "macroType", ...[
                {value: "indiv", name: i18n.t("frames:macro.individual")},
                {value: "shared", name: i18n.t("frames:macro.shared")}
        ]);
    }
    
    private get isSharedMacro(): boolean {
        return Math.floor(this.macroId / 100) > 0
    }
    
    private get humanMacroNumber(): number {
        return this.macroId % 100;
    }
    
    loadSettings(settings: MacroButtonSettings): void {
        this.macroId = settings.macroId;
        
        this._renderRadio();
        this.macroNumberField.value = this.humanMacroNumber.toString();
    }

    renderHTML(): void {
        this.domParent.append(this.macroRadioField);
        this.domParent.append(PIUtils.createPILabeledElement(i18n.t("frames:macro.number"), this.macroNumberField));
        this._renderRadio()
        
        this.macroNumberField.value = this.humanMacroNumber.toString();
        
        this.macroRadioField.onchange = this._onUpdate.bind(this);
        this.macroNumberField.oninput = this._onUpdate.bind(this);
    }
    
    private _renderRadio() {
        let choice: HTMLInputElement | null;

        if (this.isSharedMacro) {
            choice = document.querySelector(`input[name="macroType"][value="shared"]`);
        } else {
            choice = document.querySelector(`input[name="macroType"][value="indiv"]`);
        }
        
        if (choice === null) {
            return;
        }
        
        choice.checked = true;
    }
    
    private _onUpdate() {
        let typeSelector = document.querySelector('input[name="macroType"]:checked') as HTMLInputElement;
        
        if (!this.macroNumberField.value || !typeSelector) {
            // user didn't fill form out completely yet, don't save
            return;
        }
        
        if (!this.macroNumberField.validity.valid) {
            this.macroNumberField.setAttribute("style", "color: orangered");
            return;
        } else {
            this.macroNumberField.setAttribute("style", "");
        }

        let isShared = (typeSelector.value === "shared")
        
        this.macroId = parseInt(this.macroNumberField.value) + (isShared ? 100 : 0)
        
        this.setSettings({
            macroId: this.macroId
        });
    }
    
}
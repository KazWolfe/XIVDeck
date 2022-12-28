import { GearsetPayload } from "../../../../button/payloads/actions/GearsetPayload";
import i18n from "../../../../i18n/i18n";
import { PIUtils } from "../../../../util/PIUtils";
import { BaseSubsetting } from "../BaseSubsetting";

export class GearsetSettings implements BaseSubsetting {
    payload: GearsetPayload;

    private readonly _gearsetDiv: HTMLDivElement;
    private readonly _checkboxElement: HTMLElement;

    private readonly _glamourPlateIdInner: HTMLInputElement;
    private readonly _glamourPlateIdWrapped: HTMLElement;

    onUpdate?: (payload: object) => void;

    constructor(payload?: GearsetPayload) {
        this.payload = {...payload};

        this._gearsetDiv = document.createElement("div");

        this._checkboxElement = PIUtils.generateRadioSelection("", "glamourPlate", "checkbox", ...[
            {
                value: "useGlamourPlate",
                name: i18n.t("frames:action.subframes.gearset.manualPlate"),
                checked: (this.payload?.glamourPlateId != null)
            }
        ]);
        this._checkboxElement.onchange = this._onCheckChange.bind(this);
        this._gearsetDiv.append(this._checkboxElement);

        this._glamourPlateIdInner = this._buildPlateIdInput();
        this._glamourPlateIdWrapped = PIUtils.createPILabeledElement(
            i18n.t("frames:action.subframes.gearset.plateId"),
            this._glamourPlateIdInner
        );

        if (this.payload?.glamourPlateId != null) {
            this._gearsetDiv.append(this._glamourPlateIdWrapped);
        }
    }

    public getHtml(): HTMLElement {
        return this._gearsetDiv;
    }

    private _buildPlateIdInput(): HTMLInputElement {
        let input = document.createElement("input");
        input.type = "number";
        input.id = "slotField";
        input.min = "1";
        input.max = "20";
        input.value = this.payload.glamourPlateId?.toString() ?? "";
        input.onchange = this._onPlateIdChange.bind(this);

        return input;
    }

    private _onCheckChange(event: Event) {
        let element = event.target as HTMLInputElement;

        switch (element.value) {
            case "useGlamourPlate":
                return this._onToggleUseGlamourPlate(element);
            default:
                console.warn(`Unknown checkbox element during event process: ${element.name}`, event);
                return;
        }
    }

    private _onToggleUseGlamourPlate(element: HTMLInputElement) {
        if (element.checked) {
            this._gearsetDiv.append(this._glamourPlateIdWrapped);
            
            // if a value is cached in the field, we'll reapply it to allow undoing
            this._onPlateIdChange();
        } else {
            this._gearsetDiv.removeChild(this._glamourPlateIdWrapped);
            
            // remove the defined plate
            this.payload.glamourPlateId = undefined;
            
            if (this.onUpdate != null) {
                this.onUpdate(this.payload);
            }
        }
    }

    private _onPlateIdChange() {
        if (!this._glamourPlateIdInner.validity.valid) {
            this._glamourPlateIdInner.setAttribute("style", "color: orangered");
            return;
        } else {
            this._glamourPlateIdInner.setAttribute("style", "");
        }

        let parsedValue = parseInt(this._glamourPlateIdInner.value);
        if (this.onUpdate != null && !isNaN(parsedValue)) {
            this.payload.glamourPlateId = parsedValue;
            this.onUpdate(this.payload);
        }
    }
}
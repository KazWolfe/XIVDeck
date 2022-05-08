import i18n from "../i18n/i18n";

export class PIUtils {
    static generateConnectionErrorDom(): HTMLElement {
        let element: HTMLElement = document.createElement("details")
        element.className = "message caution"

        element.innerHTML = `
            <summary><span style="color: orangered">${i18n.t("common:connError.headline")}</span> ${i18n.t("common:connError.clickMore")}</summary>
                <p>${i18n.t("common:connError.checkFor")}</p>
                <ul>
                    <li>${i18n.t("common:connError.checkGameRunning")}</li>
                    <li>${i18n.t("common:connError.checkPluginInstalled")}</li>
                    <li>${i18n.t("common:connError.checkSettingsCorrect")}</li>
                </ul>
            <p>${i18n.t("common:connError.resolveSteps")}</p>
        `

        return element;
    }
    
    static createPILabeledElement(label: string, formElement: HTMLElement): HTMLElement {
        let item = document.createElement("div");
        item.className = "sdpi-item";

        let sdLabel = document.createElement("label");
        sdLabel.setAttribute("for", formElement.id);
        sdLabel.innerText = label;
        sdLabel.className = "sdpi-item-label";
        
        formElement.classList.add("sdpi-item-value");
        
        item.append(sdLabel, formElement)
        
        return item;
    }
    
    static createDefaultSelection(innerText: string = "item"): HTMLOptionElement {
        let el = document.createElement("option");

        el.value = "default";
        el.disabled = true;
        el.selected = true;
        el.innerText = innerText;

        return el;
    }
    
    static validateMinMaxOfNumberField(field: HTMLInputElement) {
        if (field.min) {
            if (parseInt(field.value) < parseInt(field.min)) {
                return false;
            }
        }

        if (field.max) {
            if (parseInt(field.value) > parseInt(field.max)) {
                return false;
            }
        }

        return true;
    }
    
    static generateRadioSelection(title: string, id: string, ...choices: RadioSelection[]): HTMLElement {
        let radioInner = document.createElement("div");
        radioInner.classList.add("sdpi-item-value");
        
        choices.forEach((rs, index) => {
            let choiceSpan = document.createElement("span");
            choiceSpan.classList.add('sdpi-item-child');
            
            let choiceInput = document.createElement("input");
            choiceInput.id = `r_${id}_choice${index}`;
            choiceInput.type = "radio";
            choiceInput.name = id;
            choiceInput.value = rs.value;
            
            let choiceLabel = document.createElement("label");
            choiceLabel.setAttribute("for", `r_${id}_choice${index}`);
            choiceLabel.classList.add("sdpi-item-label");
            choiceLabel.innerHTML = `<span></span> ${rs.name}`

            choiceSpan.append(choiceInput);
            choiceSpan.append(choiceLabel);
            
            radioInner.append(choiceSpan);
        });
        
        let labeledElement = this.createPILabeledElement(title, radioInner);
        labeledElement.setAttribute("type", "radio");
        labeledElement.id = id;
        
        return labeledElement;
    }
    
    static localizeDomTree() {
        document.querySelectorAll("*").forEach((node) => {
            let i18nKey = node.getAttribute("data-i18n");
            if (!i18nKey) return;
            
            node.innerHTML = i18n.t(i18nKey);
        })
    }
}

export interface RadioSelection {
    value: string,
    name: string
}
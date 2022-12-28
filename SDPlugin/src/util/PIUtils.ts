﻿import i18n from "../i18n/i18n";

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
        
        if (label == "") {
            sdLabel.classList.add("empty");
        }
        
        formElement.classList.add("sdpi-item-value");
        
        item.append(sdLabel, formElement)
        
        return item;
    }
    
    static createDefaultSelection(innerText: string | null | undefined): HTMLOptionElement {
        if (innerText == null) {
            innerText = "item"
        }
        
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
    
    static generateRadioSelection(title: string, id: string, type: "radio" | "checkbox" = "radio" , ...choices: RadioSelection[]): HTMLElement {
        let radioInner = document.createElement("div");
        radioInner.classList.add("sdpi-item-value");
        
        choices.forEach((rs, index) => {
            let choiceSpan = document.createElement("span");
            choiceSpan.classList.add('sdpi-item-child');
            
            let choiceInput = document.createElement("input");
            choiceInput.id = `r_${id}_choice${index}`;
            choiceInput.type = type;
            choiceInput.name = id;
            choiceInput.value = rs.value;
            if (rs.checked) choiceInput.checked = true;
            
            let choiceLabel = document.createElement("label");
            choiceLabel.setAttribute("for", `r_${id}_choice${index}`);
            choiceLabel.classList.add("sdpi-item-label");
            choiceLabel.innerHTML = `<span></span> ${rs.name}`

            choiceSpan.append(choiceInput);
            choiceSpan.append(choiceLabel);
            
            radioInner.append(choiceSpan);
        });
        
        let labeledElement = this.createPILabeledElement(title, radioInner);
        labeledElement.setAttribute("type", type);
        labeledElement.id = id;
        
        return labeledElement;
    }
    
    static generateRange(label: string, id: string, min: number, max: number, step: number): HTMLElement {
        let innerDiv = document.createElement("div");
        innerDiv.classList.add("sdpi-item-value");
        
        let lowerSpan = document.createElement("span");
        lowerSpan.setAttribute("value", min.toString());
        lowerSpan.innerText = min.toString();
        innerDiv.append(lowerSpan);
        
        let inputObj = document.createElement("input");
        inputObj.type = "range";
        inputObj.id = `${id}-input`;
        inputObj.min = min.toString();
        inputObj.max = max.toString();
        inputObj.step = step.toString();
        inputObj.setAttribute("list", `${id}-datalist`);
        innerDiv.append(inputObj);
        
        let datalist = document.createElement("datalist");
        datalist.id = `${id}-datalist`;
        
        for (var i = 0; i < ((max - min) / step) - 1; i++) {
            var opt = document.createElement("option");
            opt.innerText = (min + (i + 1) * step).toString();
            datalist.append(opt);
        }
        
        innerDiv.append(datalist);

        let upperSpan = document.createElement("span");
        upperSpan.setAttribute("value", max.toString());
        upperSpan.innerText = max.toString();
        innerDiv.append(upperSpan);

        let labeledElement = this.createPILabeledElement(label, innerDiv);
        labeledElement.setAttribute("type", "range");
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
    name: string,
    checked?: boolean
}
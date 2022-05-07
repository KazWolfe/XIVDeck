export class PIUtils {
    static generateConnectionErrorDom(): HTMLElement {
        let element: HTMLElement = document.createElement("details")
        element.className = "message caution"

        element.innerHTML = `
            <summary><span style="color: orangered">The XIVDeck Game Plugin wasn't detected!</span> Click for more info...</summary>
                <p>Please check to ensure that:</p>
                <ul>
                    <li>Final Fantasy XIV is running,</li>
                    <li>The XIVDeck Game Plugin is properly installed and configured,</li>
                    <li>The connection settings below are correct.</li>
                </ul>
            <p>After verifying all of the above, refresh this settings pane to attempt to connect to the game again.</p>
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
    
    static createDefaultSelection(type: string = "item"): HTMLOptionElement {
        let el = document.createElement("option");

        el.value = "default";
        el.disabled = true;
        el.selected = true;
        el.innerText = `Select ${type}...`;

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
    
    static generateRadioSelection(title: string, id: string, ...choices: string[]): HTMLElement {
        let radioInner = document.createElement("div");
        radioInner.classList.add("sdpi-item-value");
        
        choices.forEach((name, index) => {
            let choiceSpan = document.createElement("span");
            choiceSpan.classList.add('sdpi-item-child');
            
            let choiceInput = document.createElement("input");
            choiceInput.id = `r_${id}_choice${index}`;
            choiceInput.type = "radio";
            choiceInput.name = id;
            choiceInput.value = name;
            
            let choiceLabel = document.createElement("label");
            choiceLabel.setAttribute("for", `r_${id}_choice${index}`);
            choiceLabel.classList.add("sdpi-item-label");
            choiceLabel.innerHTML = `<span></span> ${name}`

            choiceSpan.append(choiceInput);
            choiceSpan.append(choiceLabel);
            
            radioInner.append(choiceSpan);
        });
        
        let labeledElement = this.createPILabeledElement(title, radioInner);
        labeledElement.setAttribute("type", "radio");
        labeledElement.id = id;
        
        return labeledElement;
    }
}
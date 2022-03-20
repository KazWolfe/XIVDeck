import {BaseFrame} from "../BaseFrame";
import {CommandButtonSettings} from "../../button/buttons/CommandButton";
import {PIUtils} from "../../util/PIUtils";

export class CommandFrame extends BaseFrame<CommandButtonSettings> {
    commandField: HTMLTextAreaElement;
    command: string = "/";
    
    constructor() {
        super();
        
        this.commandField = document.createElement("textarea");
    }
    
    loadSettings(settings: CommandButtonSettings): void {
        let loadedCommand = settings.command || this.command;

        this.command = loadedCommand;
        this.commandField.value = loadedCommand;
    }

    renderHTML(): void {
        this.domParent.append(PIUtils.createPILabeledElement("Command", this.commandField));
        this.commandField.oninput = this._onChange.bind(this);
    }
    
    private _onChange(event: Event) {
        let newCommand: string = this.commandField.value;

        // validation
        if (!newCommand.startsWith("/")) {
            newCommand = `/${newCommand}`
        }
        newCommand = newCommand.replace(/\n/g, " ");
        
        // write-back
        this.commandField.value = newCommand;

        // don't actually save if invalid
        if (newCommand === "/") return;
        
        this.setSettings({
            command: newCommand
        })
    }
}
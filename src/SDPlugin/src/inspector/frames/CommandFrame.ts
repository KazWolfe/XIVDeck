import {BaseFrame} from "../BaseFrame";
import {CommandButtonSettings} from "../../button/buttons/CommandButton";
import {PIUtils} from "../../util/PIUtils";
import i18n from "../../i18n/i18n";

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
        this.domParent.append(PIUtils.createPILabeledElement(i18n.t("frames:command.command-label"), this.commandField));
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
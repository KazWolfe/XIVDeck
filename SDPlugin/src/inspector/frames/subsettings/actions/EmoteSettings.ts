import { EmoteLogMode, EmotePayload } from "../../../../button/payloads/actions/EmotePayload";
import { PIUtils } from "../../../../util/PIUtils";
import { BaseSubsetting } from "../BaseSubsetting";

export class EmoteSettings implements BaseSubsetting {
    payload?: EmotePayload;

    readOnly: boolean = false;
    
    private readonly _logMessageRadio: HTMLElement;

    onUpdate?: (payload: EmotePayload) => void;

    constructor(payload: EmotePayload) {
        console.log("creating new EmoteSettings", payload);
        
        this.payload = {...payload};

        this._logMessageRadio = PIUtils.generateRadioSelection("Log Settings", "logMode", ...[
            {value: EmoteLogMode.DEFAULT, name: "Use Game Defaults", checked: true},
            {value: EmoteLogMode.ALWAYS, name: "Always Show Log Message"},
            {value: EmoteLogMode.NEVER, name: "Never Show Log Message"}
        ]);
        this._renderRadio();
        this._logMessageRadio.onchange = this._onLogSettingChange.bind(this);
    }

    public getHtml() : HTMLElement {
        return this._logMessageRadio;
    }

    private _renderRadio() {
        let element: HTMLInputElement | null =
            this._logMessageRadio.querySelector(`input[name="logMode"][value="${(this.payload?.logMode)}"]`);
        
        if (element == null) return;
        element.checked = true;
    }

    private _onLogSettingChange(event: Event) {
        let element = event.target as HTMLInputElement;
        if (!element.checked) return;
        
        if (!Object.values(EmoteLogMode).includes(element.value as EmoteLogMode)) {
            return;
        }

        this.payload = {
            logMode: element.value as EmoteLogMode,
        }

        if (this.onUpdate != null) {
            this.onUpdate(this.payload);
        }
    }
}
import { VolumeButtonMode, VolumeButtonSettings } from "../../button/settings/VolumeButtonSettings";
import i18n from "../../i18n/i18n";
import piInstance from "../../inspector";
import { FFXIVApi } from "../../link/ffxivplugin/FFXIVApi";
import { PIUtils } from "../../util/PIUtils";
import { BaseFrame } from "../BaseFrame";

export class VolumeFrame extends BaseFrame<VolumeButtonSettings> {
    // state
    isDial: boolean = false;

    // settings
    selectedChannel: string = "default";
    selectedMode: VolumeButtonMode = VolumeButtonMode.MUTE;
    selectedMultiplier: number = 0;
    selectedValue: number = 100;

    // dom
    channelSelector: HTMLSelectElement;
    buttonModeField: HTMLElement;
    valueContainer: HTMLElement;
    valueInput?: HTMLInputElement;

    constructor() {
        super();

        this.isDial = (piInstance.sdPluginLink.actionInfo?.controller == "Encoder");

        this.channelSelector = document.createElement("select");
        this.channelSelector.id = "channelSelector";
        this.channelSelector.onchange = this._onChannelChange.bind(this);
        this.channelSelector.add(PIUtils.createDefaultSelection(i18n.t("frames:volume.default-type")));

        this.buttonModeField = PIUtils.generateRadioSelection("Mode", "buttonMode", "radio", ...[
            {value: VolumeButtonMode.SET, name: "Set"},
            {value: VolumeButtonMode.ADJUST, name: "Adjust"},
            {value: VolumeButtonMode.MUTE, name: "Mute"}
        ]);
        this.buttonModeField.onchange = this._onModeChange.bind(this);
        
        this.valueContainer = document.createElement("div");
        this.valueContainer.id = "valueContainer";

        piInstance.xivPluginLink.on("_ready", this.loadGameData.bind(this));
    }

    loadSettings(settings: VolumeButtonSettings): void {
        this.selectedChannel = settings.channel ?? this.selectedChannel;
        this.channelSelector.value = this.selectedChannel;

        // Ugly hack to handle multiplier case for dials
        this.selectedMultiplier = settings.multiplier ?? (this.isDial ? 1 : 0);
        this.selectedValue = settings.value ?? 100;
        this.selectedMode = settings.mode ?? VolumeButtonMode.MUTE;
        
        this._renderRadio();
        this.renderValueInput();

        // hack in to load the last known channel (if set)
        if (this.selectedChannel != "default") {
            let typeKey = `volumetypes:${this.selectedChannel}.full`;
            let genericType = PIUtils.createDefaultSelection(i18n.t(typeKey));
            genericType.id = this.selectedChannel;
            this.channelSelector.add(genericType);
        }
    }

    renderHTML(): void {
        this.domParent.append(PIUtils.createPILabeledElement(i18n.t("frames:volume.channel"), this.channelSelector));
        
        if (!this.isDial) {
            this.domParent.append(this.buttonModeField);
        }

        this.domParent.append(this.valueContainer);
        this.renderValueInput();
    }
    
    renderValueInput() {
        let elements: { rangeDiv: HTMLElement, input: HTMLInputElement }
        let value: string;
        
        if (this.isDial) {
            elements = PIUtils.generateRange(i18n.t("frames:volume.step"), "volMultiplier", 1, 5, 1);
            value = this.selectedMultiplier.toString();
        } else if (this.selectedMode == VolumeButtonMode.SET) {
            elements = PIUtils.generateRange("Volume", "volValue", 0, 100, null);
            value = this.selectedValue.toString();
        } else if (this.selectedMode == VolumeButtonMode.ADJUST) {
            elements = PIUtils.generateRange(i18n.t("frames:volume.step"), "volMultiplier", -25, 25, null);
            value = this.selectedMultiplier.toString();
        } else {
            this.valueContainer.innerHTML = "";
            this.valueInput = undefined;
            return;
        }

        this.valueInput = elements.input;
        this.valueInput.value = value;
        this.valueInput.onchange = this._onValueChange.bind(this);

        this.valueContainer.replaceChildren(elements.rangeDiv);
    }

    private async loadGameData() {
        let volumeData = await FFXIVApi.Volume.getChannels();

        // reset the dom
        this.channelSelector.options.length = 0;
        this.channelSelector.add(PIUtils.createDefaultSelection(i18n.t("frames:volume.default-type")));

        volumeData.forEach((v, k) => {
            let option = document.createElement("option");
            let typeKey = `volumetypes:${k}.full`;
            option.value = k;
            option.innerText = i18n.t(typeKey);
            this.channelSelector.add(option);
        });

        this.channelSelector.value = this.selectedChannel;
    }
    
    private _renderRadio() {
        if (this.isDial) return;

        let element: HTMLInputElement | null =
            this.buttonModeField.querySelector(`input[name="buttonMode"][value="${(this.selectedMode)}"]`);

        if (element == null) return;
        element.checked = true;
    }
    
    private async _onChannelChange() {
        this.selectedChannel = this.channelSelector.value;

        this._save()
    }
    
    private async _onModeChange(event: Event) {
        // Mode change has no effect (and shouldn't even execute) on dials, so just ignore it.
        if (this.isDial) return;

        let element = event.target as HTMLInputElement;
        if (!element.checked) return;

        if (!Object.values(VolumeButtonMode).includes(element.value as VolumeButtonMode)) {
            return;
        }

        this.selectedMode = element.value as VolumeButtonMode;
        this.renderValueInput();
        
        this._save();
    }

    private async _onValueChange(event: Event) {
        let element = event.target as HTMLInputElement;
        
        if (this.isDial || this.selectedMode == VolumeButtonMode.ADJUST) {
            this.selectedMultiplier = parseInt(element.value);
        } else if (this.selectedMode == VolumeButtonMode.SET) {
            this.selectedValue = parseInt(element.value);
        }
        
        this._save()
    }
    
    private _save() {
        if (this.selectedChannel == "default") {
            return;
        }
        
        if (this.isDial) {
            this.setSettings({
                channel: this.selectedChannel,
                multiplier: this.selectedMultiplier
            });
            
            return;
        }
        
        let settings: VolumeButtonSettings = {
            channel: this.selectedChannel,
            mode: this.selectedMode
        }
        
        switch (this.selectedMode) {
            case VolumeButtonMode.SET:
                settings.value = this.selectedValue;
                break;
            case VolumeButtonMode.ADJUST:
                settings.multiplier = this.selectedMultiplier;
                break;
        }
        
        this.setSettings(settings);
    }
}
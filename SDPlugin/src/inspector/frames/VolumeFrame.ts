import {BaseFrame} from "../BaseFrame";
import {MacroButtonSettings} from "../../button/buttons/MacroButton";
import {PIUtils} from "../../util/PIUtils";
import i18n from "../../i18n/i18n";
import {VolumeButton, VolumeButtonSettings} from "../../button/buttons/VolumeButton";
import piInstance from "../../inspector";
import {FFXIVApi} from "../../link/ffxivplugin/FFXIVApi";

export class VolumeFrame extends BaseFrame<VolumeButtonSettings> {
    // settings
    selectedChannel: string = "default";
    selectedMultiplier: number = 1;
    
    // dom
    channelSelector: HTMLSelectElement;
    multiplierRange: HTMLElement;
    multiplierRangeInput: HTMLInputElement;

    constructor() {
        super();

        this.channelSelector = document.createElement("select");
        this.channelSelector.id = "channelSelector"
        this.channelSelector.onchange = this._onChannelChange.bind(this);
        this.channelSelector.add(PIUtils.createDefaultSelection(i18n.t("frames:volume.default-type")));
        
        this.multiplierRange = PIUtils.generateRange(i18n.t("frames:volume.step"), "volMultiplier", 1, 5, 1);
        this.multiplierRangeInput = this.multiplierRange.getElementsByTagName("input")[0];
        this.multiplierRangeInput.onchange = this._onMultiplierChange.bind(this);

        piInstance.xivPluginLink.on("_ready", this.loadGameData.bind(this));
    }
    
    loadSettings(settings: VolumeButtonSettings): void {
        this.selectedChannel = settings.channel ?? this.selectedChannel;
        this.channelSelector.value = this.selectedChannel;
        
        this.selectedMultiplier = settings.multiplier ?? 1;
        this.multiplierRangeInput.value = this.selectedMultiplier.toString();

        // hack in to load the last known channel (if set)
        if (this.selectedChannel != "default") {
            let typeKey = `volumetypes:${this.selectedChannel}`
            let genericType = PIUtils.createDefaultSelection(i18n.t(typeKey));
            genericType.id = this.selectedChannel;
            this.channelSelector.add(genericType);
        }
    }

    renderHTML(): void {
        this.domParent.append(PIUtils.createPILabeledElement(i18n.t("frames:volume.channel"), this.channelSelector));
        this.domParent.append(this.multiplierRange);
    }
    
    private async loadGameData() {
        let volumeData = await FFXIVApi.Volume.getChannels();
        
        // reset the dom
        this.channelSelector.options.length = 0;
        this.channelSelector.add(PIUtils.createDefaultSelection(i18n.t("frames:volume.default-type")));
        
        volumeData.forEach((v, k) => {
            let option = document.createElement("option");
            let typeKey = `volumetypes:${k}`
            option.value = k;
            option.innerText = i18n.t(typeKey);
            this.channelSelector.add(option);
        });

        this.channelSelector.value = this.selectedChannel;
    }
    
    private async _onChannelChange() {
        let newSelection = this.channelSelector.value;
        this.selectedChannel = this.channelSelector.value;
        
        if (newSelection == "default") {
            return;
        }
        
        this.setSettings({
            channel: this.selectedChannel,
            multiplier: this.selectedMultiplier
        })
    }
    
    private async _onMultiplierChange() {
        this.selectedMultiplier = parseInt(this.multiplierRangeInput.value);
        
        if (this.selectedChannel == "default") {
            return;
        }

        this.setSettings({
            channel: this.selectedChannel,
            multiplier: this.selectedMultiplier
        });
    }
}
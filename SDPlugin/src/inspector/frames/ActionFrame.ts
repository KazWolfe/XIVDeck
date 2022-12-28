import { EmotePayload } from "../../button/payloads/actions/EmotePayload";
import { GearsetPayload } from "../../button/payloads/actions/GearsetPayload";
import i18n from "../../i18n/i18n";
import piInstance from "../../inspector";
import { FFXIVApi } from "../../link/ffxivplugin/FFXIVApi";
import { FFXIVAction } from "../../link/ffxivplugin/GameTypes";
import { PIUtils } from "../../util/PIUtils";
import { StringUtils } from "../../util/StringUtils";
import { BaseFrame } from "../BaseFrame";
import { ActionButtonSettings } from "../../button/buttons/ActionButton";
import { EmoteSettings } from "./subsettings/actions/EmoteSettings";
import { GearsetSettings } from "./subsettings/actions/GearsetSettings";
import { BaseSubsetting } from "./subsettings/BaseSubsetting";


export class ActionFrame extends BaseFrame<ActionButtonSettings> {

    settings: ActionButtonSettings;

    // cache
    actionCache = new Map<string, FFXIVAction[]>();

    // internal state
    selectedType?: string;
    selectedAction?: number;
    selectedActionName?: string;
    subsettingsModule?: BaseSubsetting;
    
    // dom
    typeSelector: HTMLSelectElement;
    actionSelector: HTMLSelectElement;
    subsettingsDiv: HTMLDivElement;

    constructor() {
        super();
        
        this.settings = { };

        this.typeSelector = document.createElement("select");
        this.typeSelector.id = "typeSelector";
        this.typeSelector.onchange = this._onTypeChange.bind(this);

        this.actionSelector = document.createElement("select");
        this.actionSelector.id = "actionSelector";
        this.actionSelector.onchange = this._onItemChange.bind(this);
        
        this.subsettingsDiv = document.createElement("div");
        this.subsettingsDiv.id = "subsettings";

        piInstance.xivPluginLink.on("_ready", this._loadGameData.bind(this));
    }

    loadSettings(settings: ActionButtonSettings): void {
        this.settings = settings;

        this.selectedType = this.settings?.actionType;
        this.selectedAction = this.settings?.actionId;
        this.selectedActionName = this.settings?.actionName;

        this._renderTypeDropdown();
        this._renderActionDropdown();
    }

    renderHTML(): void {
        this.domParent.append(PIUtils.createPILabeledElement(i18n.t("frames:action.type"), this.typeSelector));
        this.domParent.append(PIUtils.createPILabeledElement(i18n.t("frames:action.action"), this.actionSelector));
        this.domParent.append(this.subsettingsDiv);
    }

    private async _loadGameData(): Promise<void> {
        this.actionCache = await FFXIVApi.Action.getActions();

        this._renderTypeDropdown();
        this._renderActionDropdown();
    }

    private _renderTypeDropdown(): void {
        this.typeSelector.innerHTML = ""; // clear the selector

        let placeholder = PIUtils.createDefaultSelection(i18n.t("frames:action.default-type"));
        this.typeSelector.add(placeholder);

        if (this.selectedType == null) {
            placeholder.selected = true;
        } else if (!this.actionCache.has(this.selectedType)) {
            let failsafe = PIUtils.createDefaultSelection(this.selectedType);
            failsafe.value = this.selectedType;
            failsafe.selected = true;

            this.typeSelector.add(failsafe);
        }

        this.actionCache.forEach((_, k) => {
            let typeKey = `actiontypes:${k}`;

            let element = document.createElement("option");
            element.value = k;
            element.textContent = i18n.t(typeKey);

            if (k == this.selectedType)
                element.selected = true;

            this.typeSelector.add(element);
        });
    }

    private _renderActionDropdown() {
        this.actionSelector.innerHTML = ""; // clear the selector

        let placeholder = PIUtils.createDefaultSelection(i18n.t("frames:action.default-action"));
        placeholder.selected = true;
        this.actionSelector.add(placeholder);

        // abort if there's no selected type - nothing to filter this on
        if (this.selectedType == null) return;

        let groupCache = new Map<string, HTMLOptGroupElement>();
        let filteredActions = this.actionCache.get(this.selectedType) ?? [];
        let actionFound = false;

        filteredActions.forEach((action) => {
            let parent: HTMLSelectElement | HTMLOptGroupElement = this.actionSelector;

            if (action.category) {
                if (groupCache.has(action.category)) {
                    parent = groupCache.get(action.category)!;
                } else {
                    parent = document.createElement("optgroup");
                    parent.label = StringUtils.toTitleCase(action.category);
                    this.actionSelector.append(parent);
                    groupCache.set(action.category, parent);
                }
            }

            let option = document.createElement("option");
            option.value = action.id.toString();
            option.title = StringUtils.toTitleCase(action.name ?? i18n.t("frames:action.unknown"));
            option.textContent = `[#${action.id}] ${option.title}`;

            if (this.selectedAction == action.id && (this.settings.actionType == this.selectedType)) {
                option.selected = true;
                actionFound = true;
                this._renderActionSubsettings();
            }

            parent.append(option);
        });

        // handle case of the filtered action not being returned or available
        if (!actionFound && this.selectedAction != null && (this.settings.actionType == this.selectedType)
            && (this.settings.cache != null || this.selectedActionName != null)) {

            let cachedAction = this.settings.cache;
            let cachedName = StringUtils.toTitleCase( cachedAction?.name ?? this.selectedActionName ?? i18n.t("frames:action.unknown"));

            let failsafe = document.createElement("option");
            failsafe.value = this.selectedAction.toString();
            failsafe.textContent = `[#${this.selectedAction}] ${cachedName}`;
            failsafe.disabled = true;
            failsafe.selected = true;

            if (cachedAction?.category != null) {
                let parent = document.createElement("optgroup");
                parent.label = StringUtils.toTitleCase(cachedAction.category);
                placeholder.after(parent);

                parent.append(failsafe);
            } else {
                placeholder.after(failsafe);
            }

            this._renderActionSubsettings();
        }
    }
    
    private _renderActionSubsettings() {
        this._clearSubsettingsModule();
        
        switch (this.selectedType) {
            case "Emote":
                this.subsettingsModule = new EmoteSettings(this.settings?.payload as EmotePayload);
                break;
            case "GearSet":
                this.subsettingsModule = new GearsetSettings(this.settings?.payload as GearsetPayload);
                break;
        }
        
        if (this.subsettingsModule != null) {
            this.subsettingsDiv.append(this.subsettingsModule.getHtml());
            this.subsettingsModule.onUpdate = this._onSubsettingsUpdate.bind(this);
        }
    }
    
    private _clearSubsettingsModule() {
        this.subsettingsDiv.innerHTML = "";
        this.subsettingsModule = undefined;
    }

    private _onTypeChange(event: Event): void {
        this._clearSubsettingsModule();
        
        this.selectedType = (event.target as HTMLSelectElement).value;
        this._renderActionDropdown();
    }

    private _onItemChange(event: Event): void {
        let selectedActionId = (event.target as HTMLSelectElement).value;
        
        if (selectedActionId == "default" || this.selectedType == "default" || this.selectedType == null) return;

        this.selectedAction = parseInt(selectedActionId);
        
        // initialize settings if unset
        if (this.settings == undefined) {
            this.settings = { };
        }
        
        // if type is changing, we need to reset payload in addition to all other changes.
        if (this.selectedType != this.settings.actionType) {
            this.settings.payload = undefined;
        }
        
        this.settings.actionType = this.selectedType;
        this.settings.actionId = this.selectedAction;
        this.settings.cache = this.actionCache.get(this.selectedType)?.find((e) => e.id == this.selectedAction);
        
        // ToDo: Remove this (eventually). Migration.
        this.settings.actionName = undefined;
        
        this.setSettings(this.settings);
        
        this._renderActionSubsettings();
    }
    
    private _onSubsettingsUpdate(payload: unknown) {
        if (this.selectedType == null) return;
        
        this.settings.payload = payload;
        this.setSettings(this.settings);
    }
}

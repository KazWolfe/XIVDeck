var ExecuteHotbarSlotButtonHandler = {
    type: `${PLUGIN_ID}.actions.exechotbar`,
    cache: {}, // on context -> Object

    elgatoEventHandlers: {
        // Called when the action is registered (elgato is starting up, new button added, etc.)
        willAppear: function(event) {
            if(!event.payload || !event.payload.hasOwnProperty('settings')) return;

            myAction = new ExecuteHotbarSlotButton(event);
            ExecuteHotbarSlotButtonHandler.cache[event.context] = myAction;
            myAction.requestIcon();
        },

        // Called when we expect something to no longer be on a Stream Deck. 
        willDisappear: function(event) {
            let found = ExecuteHotbarSlotButtonHandler.cache[event.context];
            if (found) {
                found.dispose();
                delete ExecuteHotbarSlotButtonHandler.cache[event.context];
            }
        },

        // Called when the user presses a Stream Deck button.
        keyDown: function(event) {
            let thisInstance = ExecuteHotbarSlotButtonHandler.cache[event.context];
            if (!thisInstance) { ExecuteHotbarSlotButtonHandler.elgatoEventHandlers.willAppear(event) }

            thisInstance.execute(event);
        },

        // Called when the user releases a Stream Deck button.
        keyUp: function(event) {
            // no-op, we prefer to respond to keyDown
        },

        // Called when a specific button receives settings. 
        didReceiveSettings: function(event) {
            let thisInstance = ExecuteHotbarSlotButtonHandler.cache[event.context];
            let receivedSettings = event.payload.settings;

            if (!thisInstance || !receivedSettings) return;

            // save settings and re-render
            thisInstance.loadSettings(receivedSettings);
            thisInstance.requestIcon();
            thisInstance.render();
        },

        renderPIPane: function (event) {
            console.debug("render", event)

            domElement = event.domElement;
            actionInfo = event.actionInfo;

            domElement.innerHTML += `
            <div class="sdpi-item">
                <div class="sdpi-item-label">Hotbar</div>
                <select class="sdpi-item-value" id="hotbarSelection">
                    <option value="default" id="hotbarPlaceholder" selected disabled>Select hotbar...</option>
                    <optgroup label="Regular Hotbars">
                        <option value="0">Hotbar 1</option>
                        <option value="1">Hotbar 2</option>
                        <option value="2">Hotbar 3</option>
                        <option value="3">Hotbar 4</option>
                        <option value="4">Hotbar 5</option>
                        <option value="5">Hotbar 6</option>
                        <option value="6">Hotbar 7</option>
                        <option value="7">Hotbar 8</option>
                        <option value="8">Hotbar 9</option>
                        <option value="9">Hotbar 10</option>
                    </optgroup>
                    <optgroup label="Cross Hotbars">
                        <option value="10">Cross Hotbar 1</option>
                        <option value="11">Cross Hotbar 2</option>
                        <option value="12">Cross Hotbar 3</option>
                        <option value="13">Cross Hotbar 4</option>
                        <option value="14">Cross Hotbar 5</option>
                        <option value="15">Cross Hotbar 6</option>
                        <option value="16">Cross Hotbar 7</option>
                        <option value="17">Cross Hotbar 8</option>
                    </optgroup>
                </select>
            </div>
            <div class="sdpi-item">
                <div class="sdpi-item-label">Slot</div>
                <input type="number" id="slotSelection" min="1" max="12" class="sdpi-item-value">
            </div>
            <div class="sdpi-item">
                <div class="sdpi-item-label">Use Game Icon</div>
                <input class="sdpi-item-value" type="checkbox" id="useGameIcon" checked>
                <label for="useGameIcon"><span></span></label>
            </div>
            `

            let domHotbarSelection = document.getElementById("hotbarSelection")
            let domSlotSelection = document.getElementById("slotSelection")

            if (actionInfo.payload.settings.hasOwnProperty("hotbarId")) {
                domHotbarSelection.value = actionInfo.payload.settings.hotbarId
            }
            if (actionInfo.payload.settings.hasOwnProperty("slotId")) {
                domSlotSelection.value = actionInfo.payload.settings.slotId + 1;
            }
            
            
            if (domHotbarSelection.value > 9) {
                domSlotSelection.setAttribute("max", "16")
            }
            
            domHotbarSelection.addEventListener('change', ExecuteHotbarSlotButtonHandler.piHandlers.onHotbarIdUpdate)
            domSlotSelection.addEventListener('change', ExecuteHotbarSlotButtonHandler.piHandlers.onSlotIdUpdate)
        }
    },

    ffxivEventHandlers: {
        hotbarIcon: function (event) {
            for (var i in Object.values(ExecuteHotbarSlotButtonHandler.cache)) {
                var instance = Object.values(ExecuteHotbarSlotButtonHandler.cache)[i]
                
                if (!(instance.hotbarId === event.hotbarId && instance.slotId === event.slotId)) {
                    continue
                }
                
                instance.icon = event.iconData;
                instance.render()
            }
        },
        
        hotbarUpdate: function (event) {
            for (var i in Object.values(ExecuteHotbarSlotButtonHandler.cache)) {
                Object.values(ExecuteHotbarSlotButtonHandler.cache)[i].requestIcon();
            }
        },
        
        initReply: function (event) {
            ExecuteHotbarSlotButtonHandler.ffxivEventHandlers.hotbarUpdate(event);
        }
    },
    
    piHandlers: { 
        onHotbarIdUpdate: function (event) {
            console.log("change", event)
            
            let domSlotSelection = document.getElementById("slotSelection")
            
            if (event.target.value > 9) {
                domSlotSelection.setAttribute("max", "16")
            } else {
                domSlotSelection.setAttribute("max", "12")

                if (domSlotSelection.value > 12) {
                    domSlotSelection.value = 12
                }
            }

            actionInfo = $SD.actionInfo
            settings = actionInfo.payload.settings
            
            settings.hotbarId = parseInt(event.target.value)
            
            $SD.api.setSettings($XIV.uuid, settings)
        },
        
        onSlotIdUpdate: function (event) {
            actionInfo = $SD.actionInfo
            settings = actionInfo.payload.settings

            settings.slotId = parseInt(event.target.value) - 1

            $SD.api.setSettings($XIV.uuid, settings)
        }
    }
}

class ExecuteHotbarSlotButton {
    myContext = null;
    
    hotbarId = null;
    slotId = null;
    
    useGameIcon = true;
    icon = null;

    constructor(onWillAppearEvent) {
        this.myContext = onWillAppearEvent.context;
        
        if (onWillAppearEvent.payload.settings) {
            this.loadSettings(onWillAppearEvent.payload.settings);
        }
    }

    // Load in settings provided by the Elgato API
    loadSettings(settings) {
        this.hotbarId = settings.hotbarId;
        this.slotId = settings.slotId;
        
        // todo: add setting for useGameIcon, default true
    }

    // Trigger a specific action to execute itself
    execute(onKeyEvent) {
        // use settings from here rather than the plugin itself as they're more up to date here
        let settings = onKeyEvent.payload.settings;

        if (settings.hotbarId == null || settings.slotId == null) {
            throw Error("No hotbar slot was configured for this command!");
        }

        let message = { "opcode": "execHotbar", "hotbarId": settings.hotbarId, "slotId": settings.slotId }

        window.$XIV.send(message, true);
        console.info(`Triggered hotbar ${settings.hotbarId} slot ${settings.slotId} for execution by XIVDeck FFXIV plugin`);
    }
    
    render() {
        // if the game icon is disabled, reset to whatever the stream deck wants
        if (!this.useGameIcon) {
            $SD.api.setImage(this.myContext, null, '');
        }
        
        if (this.myContext && this.icon) {
            $SD.api.setImage(this.myContext, this.icon, '');
        }
    }
    
    requestIcon() {
        if (!this.useGameIcon) return;
        
        if (this.hotbarId == null || this.slotId == null) {
            return;
        }
        
        let iconRequest = { "opcode": "getHotbarIcon", "hotbarId": this.hotbarId, "slotId": this.slotId }
        $XIV.send(iconRequest);
    }

    // Clean up this action (if necessary)
    dispose() {
        // no-op for this action
    }
}

window.RA = window.RA ? window.RA.concat(ExecuteHotbarSlotButtonHandler) : [ ExecuteHotbarSlotButtonHandler ]

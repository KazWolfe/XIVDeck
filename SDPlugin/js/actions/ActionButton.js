let unlockedActions = {}

var ExecuteActionButtonHandler = {
    type: `${PLUGIN_ID}.actions.execaction`,
    cache: {}, // on context -> Object

    elgatoEventHandlers: {
        // Called when the action is registered (elgato is starting up, new button added, etc.)
        willAppear: function(event) {
            if(!event.payload || !event.payload.hasOwnProperty('settings')) return;

            myAction = new ExecuteActionButton(event);
            ExecuteActionButtonHandler.cache[event.context] = myAction;
            myAction.requestIcon();
        },

        // Called when we expect something to no longer be on a Stream Deck. 
        willDisappear: function(event) {
            let found = ExecuteActionButtonHandler.cache[event.context];
            if (found) {
                found.dispose();
                delete ExecuteActionButtonHandler.cache[event.context];
            }
        },

        // Called when the user presses a Stream Deck button.
        keyDown: function(event) {
            let thisInstance = ExecuteActionButtonHandler.cache[event.context];
            if (!thisInstance) { ExecuteActionButtonHandler.elgatoEventHandlers.willAppear(event) }

            thisInstance.execute(event);
        },

        // Called when the user releases a Stream Deck button.
        keyUp: function(event) {
            // no-op, we prefer to respond to keyDown
        },

        // Called when a specific button receives settings. 
        didReceiveSettings: function(event) {
            let thisInstance = ExecuteActionButtonHandler.cache[event.context];
            let receivedSettings = event.payload.settings;

            if (!thisInstance || !receivedSettings) return;

            // save settings and re-render
            thisInstance.loadSettings(receivedSettings);
            thisInstance.requestIcon();
            thisInstance.render();
        },

        renderPIPane: function (event) {
            domElement = event.domElement;
            actionInfo = event.actionInfo;

            domElement.innerHTML += `
            <div class="sdpi-item">
                <div class="sdpi-item-label">Action Type</div>
                <select class="sdpi-item-value" id="acTypeSelection">
                    <option value="default" id="typePlaceholder" disabled selected>Select type...</option>
                </select>
            </div>
            <div class="sdpi-item">
                <div class="sdpi-item-label">Action Name</div>
                <select class="sdpi-item-value" id="acActionSelection">
                    <option value="default" id="namePlaceholder" disabled selected>Select item...</option>
                </select>
            </div>
            `
            window.$XIV.eventManager.on("initReply", () => {
                window.$XIV.send({"opcode": "getUnlockedActions"})
            });
        }
    },

    ffxivEventHandlers: {
        actionIcon: function (event) {
            for (var i in Object.values(ExecuteActionButtonHandler.cache)) {
                var instance = Object.values(ExecuteActionButtonHandler.cache)[i]

                if (!(instance.actionType === event.action.type && instance.actionId === event.action.id)) {
                    continue
                }

                instance.icon = event.iconData;
                instance.render()
            }
        },

        initReply: function (event) {
            for (var i in Object.values(ExecuteActionButtonHandler.cache)) {
                Object.values(ExecuteActionButtonHandler.cache)[i].requestIcon();
            }
        },
        
        unlockedActions: function (event) {
            let actionInfo = $SD.actionInfo
            let settings = actionInfo.payload.settings
            unlockedActions = event.data
            
            let acTypeSelector = document.getElementById('acTypeSelection')
            
            // do nothing if not in PI
            if (!acTypeSelector) return
            
            let actionSelectorCategories = {}
            
            for (let type of Object.keys(unlockedActions)) {
                acTypeSelector.innerHTML += `<option value="${type}">${type}</option>`
            }
            
            if (settings.actionType) {
                acTypeSelector.value = settings.actionType;
                let acActionSelector = document.getElementById('acActionSelection')

                for (let action of unlockedActions[settings.actionType]) {
                    let parent = acActionSelector;
                    if (action.category) {
                        if (!actionSelectorCategories.hasOwnProperty(action.category)) {
                            let categoryDom = document.createElement("optgroup")
                            categoryDom.label = action.category
                            
                            acActionSelector.append(categoryDom)
                            actionSelectorCategories[action.category] = categoryDom;
                        }
                        
                        parent = actionSelectorCategories[action.category]
                    }
                    
                    let actionSelection = document.createElement("option")
                    actionSelection.value = action.id
                    actionSelection.innerText = `[#${action.id}] ${Utils.toTitleCase(action.name)}`
                    
                    parent.append(actionSelection);
                }
                
                if (settings.actionId) {
                    acActionSelector.value = settings.actionId
                }

                acActionSelector.addEventListener('change', ExecuteActionButtonHandler.piHandlers.onActionUpdate)
            }

            acTypeSelector.addEventListener('change', ExecuteActionButtonHandler.piHandlers.onTypeUpdate)

        },

        stateUpdate: function (event) {
            for (var i in Object.values(ExecuteActionButtonHandler.cache)) {
                let actionButon = Object.values(ExecuteActionButtonHandler.cache)[i];
                
                if (actionButon.actionType === event.type) {
                    actionButon.requestIcon();
                }
            }
        }
    },

    piHandlers: {
       onTypeUpdate: function (event) {
           console.log("change", event)
           
           let acActionSelector = document.getElementById('acActionSelection')
           
           // reset the action selector
           acActionSelector.removeEventListener('change', ExecuteActionButtonHandler.piHandlers.onActionUpdate)
           acActionSelector.innerHTML = '<option value="default" id="namePlaceholder" selected disabled>Select item...</option>'

           for (let action of unlockedActions[event.target.value]) {
               acActionSelector.innerHTML += `<option value="${action.id}">[#${action.id}] ${Utils.toTitleCase(action.name)}</option>`
           }

           acActionSelector.addEventListener('change', ExecuteActionButtonHandler.piHandlers.onActionUpdate)
       },
       
       onActionUpdate: function (event) {
           let actionInfo = $SD.actionInfo
           let settings = actionInfo.payload.settings

           settings.actionType = document.getElementById('acTypeSelection').value
           settings.actionId = parseInt(event.target.value)

           $SD.api.setSettings($XIV.uuid, settings)
       }
    }
}

class ExecuteActionButton {
    myContext = null;

    actionType = null;
    actionId = null;

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
        this.actionId = settings.actionId;
        this.actionType = settings.actionType;
        
        // this.useGameIcon = settings.useGameIcon;
    }

    // Trigger a specific action to execute itself
    execute(onKeyEvent) {
        // use settings from here rather than the plugin itself as they're more up to date here
        let settings = onKeyEvent.payload.settings;

        if (settings.actionType == null || settings.actionId == null) {
            throw Error("Not action type/ID was defined for this button!");
        }

        let message = { 
            "opcode": "execAction", 
            "sdContext": onKeyEvent.context,
            "action": { 
                "type": settings.actionType, 
                "id": settings.actionId 
            } 
        }

        window.$XIV.send(message, true);
        console.info(`Triggered ${settings.actionType} action ID ${settings.actionId}`);
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

        if (this.actionType == null || this.actionId == null) {
            return;
        }

        let iconRequest = { "opcode": "getActionIcon", "action": { "type": this.actionType, "id": this.actionId } }
        $XIV.send(iconRequest);
    }

    // Clean up this action (if necessary)
    dispose() {
        // no-op for this action
    }
}

window.RA = window.RA ? window.RA.concat(ExecuteActionButtonHandler) : [ ExecuteActionButtonHandler ]

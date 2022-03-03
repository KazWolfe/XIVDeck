var ExecuteMacroButtonHandler = {
    type: `${PLUGIN_ID}.actions.execmacro`,
    cache: {}, // on context -> Object

    elgatoEventHandlers: {
        // Called when the action is registered (elgato is starting up, new button added, etc.)
        willAppear: function(event) {
            if(!event.payload || !event.payload.hasOwnProperty('settings')) return;

            myAction = new ExecuteMacroButton(event);
            ExecuteMacroButtonHandler.cache[event.context] = myAction;
            myAction.requestIcon();
        },

        // Called when we expect something to no longer be on a Stream Deck. 
        willDisappear: function(event) {
            let found = ExecuteMacroButtonHandler.cache[event.context];
            if (found) {
                found.dispose();
                delete ExecuteMacroButtonHandler.cache[event.context];
            }
        },

        // Called when the user presses a Stream Deck button.
        keyDown: function(event) {
            let thisInstance = ExecuteMacroButtonHandler.cache[event.context];
            if (!thisInstance) { ExecuteMacroButtonHandler.elgatoEventHandlers.willAppear(event) }

            thisInstance.execute(event);
        },

        // Called when the user releases a Stream Deck button.
        keyUp: function(event) {
            // no-op, we prefer to respond to keyDown
        },

        // Called when a specific button receives settings. 
        didReceiveSettings: function(event) {
            let thisInstance = ExecuteMacroButtonHandler.cache[event.context];
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
            <div type="radio" class="sdpi-item" id="macroType">
                <div class="sdpi-item-label">Macro Type</div>
                <div class="sdpi-item-value">
                    <span class="sdpi-item-child">
                        <input id="mIndiv" type="radio" name="macroType" value="individual">
                        <label for="mIndiv" class="sdpi-item-label"><span></span>Individual</label>
                    </span>
                    <span class="sdpi-item-child">
                        <input id="mShared" type="radio" name="macroType" value="shared">
                        <label for="mShared" class="sdpi-item-label"><span></span>Shared</label>
                    </span>
                </div>
            </div>
            <div class="sdpi-item">
                <div class="sdpi-item-label">Macro Number</div>
                <input class="sdpi-item-value" id="macroNumber" min="0" max="99">
            </div>
            `

            let sharedDom = document.getElementById('macroType')
            let numberDom = document.getElementById('macroNumber')
            
            if (actionInfo.payload.settings.hasOwnProperty('macroId') && actionInfo.payload.settings.macroId !== null) {
                let userMacroNumber = actionInfo.payload.settings.macroId % 100
                let shareState = (actionInfo.payload.settings.macroId / 100) > 0 ? "shared" : "individual"
                
                let sharedDom = document.querySelector(`input[name="macroType"][value="${shareState}"]`)
                
                sharedDom.checked = true;
                numberDom.value = userMacroNumber;
            }
            
            numberDom.addEventListener('change', ExecuteMacroButtonHandler.piHandlers.updateValue)
            sharedDom.addEventListener('change', ExecuteMacroButtonHandler.piHandlers.updateValue)
        }
    },

    ffxivEventHandlers: {
        actionIcon: function (event) {
            for (var i in Object.values(ExecuteMacroButtonHandler.cache)) {
                var instance = Object.values(ExecuteMacroButtonHandler.cache)[i]

                if (!("Macro" === event.action.type && instance.macroId === event.action.id)) {
                    continue
                }

                instance.icon = event.iconData;
                instance.render()
            }
        },

        initReply: function (event) {
            for (var i in Object.values(ExecuteMacroButtonHandler.cache)) {
                Object.values(ExecuteMacroButtonHandler.cache)[i].requestIcon();
            }
        },

        stateUpdate: function (event) {
            for (var i in Object.values(ExecuteMacroButtonHandler.cache)) {
                Object.values(ExecuteMacroButtonHandler.cache)[i].requestIcon();
            }
        }
    },

    piHandlers: {
        updateValue: function (event) {
            let sharedDom = document.querySelector('input[name="macroType"]:checked')
            let numberDom = document.getElementById('macroNumber')

            let actionInfo = $SD.actionInfo
            let settings = actionInfo.payload.settings
            
            if (!numberDom.value || !sharedDom) {
                // user did not fill out the form yet fully, don't save
                return
            }
            
            // todo: verify that macro value is between 0-99, error otherwise
            
            let isShared = (sharedDom.value === "shared")
            
            settings.macroId = parseInt(numberDom.value) + (isShared ? 100 : 0)

            $SD.api.setSettings($XIV.uuid, settings)
        }
    }
}

class ExecuteMacroButton {
    myContext = null;
    
    macroId = null;

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
        this.macroId = settings.macroId;
        
        // this.useGameIcon = settings.useGameIcon;
    }

    // Trigger a specific action to execute itself
    execute(onKeyEvent) {
        // use settings from here rather than the plugin itself as they're more up to date here
        let settings = onKeyEvent.payload.settings;

        if (settings.macroId == null) {
            throw Error("Macro configuration for this button was missing!");
        }

        let message = { 
            "opcode": "execAction", 
            "sdContext": onKeyEvent.context,
            "action": { 
                "type": "Macro", 
                "id": settings.macroId
            } 
        }

        window.$XIV.send(message, true);
        console.info(`Triggered ${(((settings.macroId / 100) > 0) ? 'shared' : 'individual')} macro ID ${settings.macroId % 100}`);
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

        if (this.macroId == null) {
            return;
        }

        let iconRequest = { "opcode": "getActionIcon", "action": { "type": "Macro", "id": this.macroId } }
        $XIV.send(iconRequest);
    }

    // Clean up this action (if necessary)
    dispose() {
        // no-op for this action
    }
}

window.RA = window.RA ? window.RA.concat(ExecuteMacroButtonHandler) : [ ExecuteMacroButtonHandler ]

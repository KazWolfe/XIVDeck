var ExecuteActionActionHandler = {
    type: `${PLUGIN_ID}.actions.execaction`,
    cache: {}, // on context -> Object

    elgatoEventHandlers: {
        // Called when the action is registered (elgato is starting up, new button added, etc.)
        willAppear: function(event) {
            if(!event.payload || !event.payload.hasOwnProperty('settings')) return;

            myAction = new ExecuteActionAction(event);
            ExecuteActionActionHandler.cache[event.context] = myAction;
            myAction.requestIcon();
        },

        // Called when we expect something to no longer be on a Stream Deck. 
        willDisappear: function(event) {
            let found = ExecuteActionActionHandler.cache[event.context];
            if (found) {
                found.dispose();
                delete ExecuteActionActionHandler.cache[event.context];
            }
        },

        // Called when the user presses a Stream Deck button.
        keyDown: function(event) {
            let thisInstance = ExecuteActionActionHandler.cache[event.context];
            if (!thisInstance) { ExecuteActionActionHandler.elgatoEventHandlers.willAppear(event) }

            thisInstance.execute(event);
        },

        // Called when the user releases a Stream Deck button.
        keyUp: function(event) {
            // no-op, we prefer to respond to keyDown
        },

        // Called when a specific button receives settings. 
        didReceiveSettings: function(event) {
            let thisInstance = ExecuteActionActionHandler.cache[event.context];
            let receivedSettings = event.payload.settings;

            if (!thisInstance || !receivedSettings) return;

            // save settings and re-render
            thisInstance.loadSettings(receivedSettings);
            thisInstance.requestIcon();
            thisInstance.render();
        },

        renderPIPane: function (event) {
            // todo
        }
    },

    ffxivEventHandlers: {
        actionIcon: function (event) {
            for (var i in Object.values(ExecuteActionActionHandler.cache)) {
                var instance = Object.values(ExecuteActionActionHandler.cache)[i]

                if (!(instance.actionType === event.action.type && instance.actionId === event.action.id)) {
                    continue
                }

                instance.icon = event.iconData;
                instance.render()
            }
        },

        initReply: function (event) {
            for (var i in Object.values(ExecuteActionActionHandler.cache)) {
                Object.values(ExecuteActionActionHandler.cache)[i].requestIcon();
            }
        }
    },

    piHandlers: {
       
    }
}

class ExecuteActionAction {
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

        let message = { "opcode": "execAction", "action": { "type": settings.actionType, "id": settings.actionId } }

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

window.RA = window.RA ? window.RA.concat(ExecuteActionActionHandler) : [ ExecuteActionActionHandler ]

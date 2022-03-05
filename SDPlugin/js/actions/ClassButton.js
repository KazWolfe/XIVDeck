var ClassButtonHandler = {
    type: `${PLUGIN_ID}.actions.switchclass`,
    cache: {}, // on context -> Object
    gameClassCache: [],

    elgatoEventHandlers: {
        // Called when the action is registered (elgato is starting up, new button added, etc.)
        willAppear: function(event) {
            if(!event.payload || !event.payload.hasOwnProperty('settings')) return;

            myAction = new ClassButton(event);
            ClassButtonHandler.cache[event.context] = myAction;
            myAction.render()
        },

        // Called when we expect something to no longer be on a Stream Deck. 
        willDisappear: function(event) {
            let found = ClassButtonHandler.cache[event.context];
            if (found) {
                found.dispose();
                delete ClassButtonHandler.cache[event.context];
            }
        },

        // Called when the user presses a Stream Deck button.
        keyDown: function(event) {
            let thisInstance = ClassButtonHandler.cache[event.context];
            if (!thisInstance) { ClassButtonHandler.elgatoEventHandlers.willAppear(event) }

            thisInstance.execute(event);
        },

        // Called when the user releases a Stream Deck button.
        keyUp: function(event) {
            // no-op, we prefer to respond to keyDown
        },

        // Called when a specific button receives settings. 
        didReceiveSettings: function(event) {
            let thisInstance = ClassButtonHandler.cache[event.context];
            let receivedSettings = event.payload.settings;

            if (!thisInstance || !receivedSettings) return;

            // save settings and re-render
            thisInstance.loadSettings(receivedSettings);
            thisInstance.render();
        },

        renderPIPane: function (event) {
            console.log("render", event)
            
            domElement = event.domElement;
            actionInfo = event.actionInfo;

            domElement.innerHTML += `
            <div class="sdpi-item">
                <div class="sdpi-item-label">Class</div>
                <select class="sdpi-item-value" id="classSelection">
                    <option value="default" id="classPlaceholder" disabled selected>Select class...</option>
                </select>
            </div>
            `

            let classSelectElement = document.getElementById('classSelection')
            classSelectElement.addEventListener('change', ClassButtonHandler.piHandlers.onClassChange)
        }
    },

    ffxivEventHandlers: {
        initReply: function (event) {
            $XIV.send({"opcode": "getClasses"})
        },
        
        gameClasses: function (event) {
            ClassButtonHandler.gameClassCache = event.classes

            // instruct existing buttons to update their icons
            for (var i in Object.values(ClassButtonHandler.cache)) {
                var instance = Object.values(ClassButtonHandler.cache)[i]
                
                instance.render()
            }
            
            // and then work on the PI (if loaded)
            let domSelector = document.getElementById('classSelection')
            if (!domSelector) return;
            
            let domTypeMap = {}
            
            for (let i in ClassButtonHandler.gameClassCache) {
                let gameClass = ClassButtonHandler.gameClassCache[i]
                
                // create a group dom element if it doesn't already exist
                if (!domTypeMap.hasOwnProperty(gameClass.categoryName)) {
                    let groupDom = document.createElement("optgroup")
                    groupDom.label = gameClass.categoryName
                    
                    domSelector.appendChild(groupDom)
                    domTypeMap[gameClass.categoryName] = groupDom
                }
                
                // add the class to the list if a gearset exists for it
                if (event.available.indexOf(gameClass.id) >= 0) {
                    console.log(`${gameClass.id} in ${event.available}`, gameClass, event.available)
                    let groupDom = domTypeMap[gameClass.categoryName]
                    
                    let optionDom = document.createElement("option")
                    optionDom.value = gameClass.id
                    optionDom.innerText = Utils.toTitleCase(gameClass.name)
                    
                    groupDom.appendChild(optionDom)
                }
            }
            
            let selectedClassId = $SD.actionInfo.payload.settings.classId
            if (!(selectedClassId === null || selectedClassId === undefined)) {
                domSelector.value = selectedClassId
            }
        }
        
        
    },

    piHandlers: {
        onClassChange: function (event) {
            console.debug("class change event", event)
            actionInfo = $SD.actionInfo
            settings = actionInfo.payload.settings
            settings.classId = parseInt(event.target.value)
            
            $SD.api.setSettings($SD.uuid, settings)
        }
    }
}

class ClassButton {
    myContext = null;

    classId = null;

    useGameIcon = true;

    constructor(onWillAppearEvent) {
        this.myContext = onWillAppearEvent.context;

        if (onWillAppearEvent.payload.settings) {
            this.loadSettings(onWillAppearEvent.payload.settings);
        }
    }

    // Load in settings provided by the Elgato API
    loadSettings(settings) {
        this.classId = settings.classId;
        
        // this.useGameIcon = settings.useGameIcon;
    }

    // Trigger a specific action to execute itself
    execute(onKeyEvent) {
        // use settings from here rather than the plugin itself as they're more up to date here
        let settings = onKeyEvent.payload.settings;

        if (settings.classId == null) {
            throw Error("No class was defined for this button!");
        }

        let message = { 
            "opcode": "switchClass", 
            "sdContext": onKeyEvent.context,
            "id": settings.classId
        }

        window.$XIV.send(message, true);
        console.info(`Sent command to switch to class ${settings.classId}`);
    }

    render() {
        console.log("button render")
        
        // if the game icon is disabled, reset to whatever the stream deck wants
        if (!this.useGameIcon) {
            $SD.api.setImage(this.myContext, null, '');
        }
        
        // if game cache does not contain an icon, don't overwrite what may currently be cached.
        if (ClassButtonHandler.gameClassCache[this.classId] == null) {
            return
        }
        
        $SD.api.setImage(this.myContext, ClassButtonHandler.gameClassCache[this.classId].iconString, '');
    }
    
    

    // Clean up this action (if necessary)
    dispose() {
        // no-op for this action
    }
}

window.RA = window.RA ? window.RA.concat(ClassButtonHandler) : [ ClassButtonHandler ]

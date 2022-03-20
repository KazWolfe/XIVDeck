var CommandButtonHandler = {
    type: `${PLUGIN_ID}.actions.sendcommand`,
    
    elgatoEventHandlers: {
        keyDown: function (event) {
            console.debug("[XIVDeck.CommandButton] Got keyDown event", event);

            let command = event.payload.settings.command || '';
            if (!command) {
                console.warn("No command set, aborting command execution", event);
                throw new Error("No command was set for this command")
            }

            let message = { 
                "opcode": "command", 
                "sdContext": event.context,
                "command": command 
            };
            window.$XIV.send(message, true);

            console.debug("[XIVDeck.CommandButton] Sent command op to XIVDeck FFXIV Plugin", message)
        },

        renderPIPane: function (event) {
            console.debug("render", event)
            
            domElement = event.domElement;
            actionInfo = event.actionInfo;
            
            domElement.innerHTML += `
            <div class="sdpi-item">
                <div class="sdpi-item-label">Command</div>
                <span class="sdpi-item-value textarea">
                    <textarea type="textarea" id="command" maxlength="500">${actionInfo.payload.settings.command || ''}</textarea>
                </span>
            </div>
            `
            
            let commandElement = document.getElementById('command')
            commandElement.addEventListener('change', CommandButtonHandler.piHandlers.onCommandChange)
        }
    },
    
    piHandlers: {
        onCommandChange: function (event) {
            console.debug("command change event", event)
            actionInfo = $SD.actionInfo
            settings = actionInfo.payload.settings
            settings.command = event.target.value
            
            $SD.api.setSettings($SD.uuid, settings)
        }
    }
}


window.RA = window.RA ? window.RA.concat(CommandButtonHandler) : [ CommandButtonHandler ]

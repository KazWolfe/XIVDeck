$SD.on('connected', conn => connected(conn))

function connected(jsn) {
    console.debug('Connected Inspector:', jsn);
    let actionInfo = JSON.parse(jsn.actionInfo)
    $SD.actionInfo = actionInfo;
    let globalSettings = {}
    
    $SD.api.getGlobalSettings();

    // todo: figure out some way to move this to global settings
    window.$XIV = new FFXIVPluginLink(37984)
    
    $SD.on("didReceiveGlobalSettings", event => {
        $SD.globalSettings = event.payload.settings;
    })

    $SD.on("applicationDidLaunch", _ => {
        $XIV.isGameAlive = true;
        $XIV.connect();
    });

    $SD.on("applicationDidTerminate", _ => {
        $XIV.isGameAlive = false;

        // close the websocket just in case somehow it's not already closed
        if ($XIV.websocket) $XIV.websocket.close();
    });

    for (let action in window.RA) {
        action = window.RA[action]

        for (let ev in action.elgatoEventHandlers) {
            $SD.on(`${action.type}.${ev}`, event => {
                try {
                    action.elgatoEventHandlers[ev](event);
                } catch (ex) {
                    // $SD.api.showAlert(event.context);
                    throw ex;
                }
            })
            console.debug(`[XIVDeck - Dispatch Manager] Registered StreamDeck action: ${action.type}.${ev}`)
        }

        for (let ev in action.ffxivEventHandlers) {
            $XIV.eventManager.on(ev, event => action.ffxivEventHandlers[ev](event))
            console.debug(`[XIVDeck - Dispatch Manager] Registered FFXIV action ${ev} to ${action.type}`)
        }
    }

    var placeholderDomElement = document.getElementById("piPlaceholder")
    placeholderDomElement.innerHTML += 
        "<div class=\"sdpi-item\">" +
        `<p>${actionInfo.action}</p>` +
        "</div>"

    // emit the action to allow the ActionManager to take over
    renderEvent = { "actionInfo": actionInfo, "domElement": placeholderDomElement }
    $SD.emit(actionInfo.action + ".renderPIPane", renderEvent)
}

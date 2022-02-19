$SD.on('connected', conn => connected(conn))

function connected(jsn) {
    console.debug('Connected Plugin:', jsn);

    // todo: figure out some way to move this to global settings
    window.$XIV = new FFXIVPluginLink(37984)

    $SD.on("didReceiveGlobalSettings", event => {
        $SD.globalSettings = event.payload.settings;
    })

    $SD.on("applicationDidLaunch", _ => {
        $XIV.isGameAlive = true;

        console.log("[XIVDeck] Detected that FFXIV has opened, connecting")
        $XIV.connect();
    });

    $SD.on("applicationDidTerminate", _ => {
        $XIV.isGameAlive = false;

        console.log("[XIVDeck] Detected that FFXIV has closed, terminating WebSockets")
        
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
                    $SD.api.showAlert(event.context);
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
}
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

    for (let buttonHandler in window.RA) {
        buttonHandler = window.RA[buttonHandler]

        for (let ev in buttonHandler.elgatoEventHandlers) {
            $SD.on(`${buttonHandler.type}.${ev}`, event => {
                try {
                    buttonHandler.elgatoEventHandlers[ev](event);
                } catch (ex) {
                    $SD.api.showAlert(event.context);
                    throw ex;
                }
            })
            console.debug(`[XIVDeck - Dispatch Manager] Registered StreamDeck button listener: ${buttonHandler.type}.${ev}`)
        }

        for (let ev in buttonHandler.ffxivEventHandlers) {
            $XIV.eventManager.on(ev, event => buttonHandler.ffxivEventHandlers[ev](event))
            console.debug(`[XIVDeck - Dispatch Manager] Registered FFXIV message ${ev} to ${buttonHandler.type}`)
        }
    }
}
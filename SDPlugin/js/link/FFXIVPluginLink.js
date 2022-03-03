class FFXIVPluginLink {
    constructor(port) {
        this.port = port;
        
        this.eventManager = ELGEvents.eventEmitter("xivLink");
        this.websocket = null;
        
        this.isGameAlive = false; 
    }
    
    send(payload, interactive = false) {
        if (!this.isGameAlive && interactive) { 
            throw Error("Game is not running!"); 
        }
        
        if (!this.websocket && !interactive) return;
        
        this.websocket.sendJSON(payload);
    }

    connect(doRetry = true) {
        // if the game isn't alive, don't bother connecting
        if (!this.isGameAlive) {
            console.warn("[XIVDeck - FFXIVPluginLink] Connect ran while game was dead")
            return
        }
        
        if (this.websocket) {
            // If the websocket is already ready and working, do nothing.
            if (this.websocket.readyState) return;
            
            // Otherwise, pass through to closure and let it handle cleaning this up
            this.websocket.close();
            return
        }
        
        this.websocket = new WebSocket("ws://localhost:" + this.port + "/xivdeck");
        
        this.websocket.onopen = () => {
            this.websocket.sendJSON({ "opcode": "init", "version": "0.0.2" });  // todo: read plugin version
            this.isGameAlive = true;

            this.eventManager.emit("_wsOpened", {});
            console.info("[XIVDeck - FFXIVPluginLink] Connection established!")
        }
        
        this.websocket.onmessage = (message) => {
            let jsonObj = Utils.parseJson(message.data);
            console.debug("[XIVDeck - FFXIVPluginLink] Got websocket message", jsonObj)

            if (!jsonObj.hasOwnProperty('messageType')) {
                console.warn("[XIVDeck - FFXIVPluginLink] Malformed message from XIV Plugin, missing messageType", jsonObj);
                return
            }

            this.eventManager.emit(jsonObj.messageType, jsonObj);
        }

        /* WebSocket onClose will fire when the socket closes, but also when it fails entirely (e.g. ECONNREFUSED).
         * We'll leverage this to handle our retry logic. */
        this.websocket.onclose = (event) => {
            console.debug("[XIVDeck - FFXIVPluginLink] Connection to WebSocket server lost!")
            this.eventManager.emit("_wsClosed", {});
            this.websocket = null;
            
            // do not retry if this was a result of a server-side closure
            if (event.code === 1002 || event.code === 1008) {
                console.warn(`[XIVDeck - FFXIVPluginLink] WebSocket connection was forcibly closed with code ${event.code}, not retrying`)
                return
            }
            
            // allow us to control retry logic
            if (!doRetry) return;
            
            // if the game is reported as dead, stop trying to reconnect, it's just a waste of time.
            if (this.isGameAlive === false) {
                return;
            }

            setTimeout(this.connect.bind(this), 500);
        }
    }
}
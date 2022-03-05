const StreamDeckLink = (function() { 
    var instance;
    
    function init() {
        var uuid
        var elgatoWebsocket = null;
        var globalSettings;
        var filterPIMessages = ['didReceiveSettings', 'didReceiveGlobalSettings', 'propertyInspectorDidAppear', 'propertyInspectorDidDisappear'];
        
        var events = ELGEvents.eventEmitter("sdLink");

        function connectElgatoWS(port, uuid, messageType, applicationInfo, actionInfo) {
            this.uuid = uuid;
            applicationInfo = JSON.parse(applicationInfo);

            // kill any existing websockets
            if (elgatoWebsocket) {
                elgatoWebsocket.close();
                elgatoWebsocket = null;
            }

            elgatoWebsocket = new WebSocket("ws://127.0.0.1:" + port)
            elgatoWebsocket.onopen = function () {
                elgatoWebsocket.sendJSON({"event": messageType, "uuid": uuid})
                $SD.api.getGlobalSettings();

                $SD.uuid = uuid;
                $SD.actionInfo = actionInfo;
                $SD.applicationInfo = applicationInfo;
                $SD.messageType = messageType;
                $SD.connection = elgatoWebsocket;
                $SD.globalSettings = {};

                instance.emit('connected', {
                    connection: elgatoWebsocket,
                    port: port,
                    uuid: uuid,
                    actionInfo: actionInfo,
                    applicationInfo: applicationInfo,
                    messageType: messageType
                });
            }

            elgatoWebsocket.onerror = function (evt) {
                console.warn("[XIVDeck] Elgato WebSocket Error", evt, evt.data)
            }

            elgatoWebsocket.onclose = function (evt) {
                var reason = WEBSOCKETERROR(evt);
                console.warn("[XIVDeck] Elgato WebSocket was closed", evt, evt.data);
            }

            elgatoWebsocket.onmessage = function (evt) {
                var jsonObj = Utils.parseJson(evt.data), m;
                console.debug("[XIVDeck - StreamDeckLink] Got websocket message", jsonObj)
                
                if (!jsonObj.hasOwnProperty('action')) {
                    m = jsonObj.event;
                } else {
                    const e = jsonObj['event'];
                    if (filterPIMessages.includes(e)) {
                        events.emit(e, jsonObj);
                    }
                    switch (messageType) {
                        case 'registerPlugin':
                            m = jsonObj['action'] + '.' + jsonObj['event'];
                            break;
                        case 'registerPropertyInspector':
                            m = 'sendToPropertyInspector';
                            break;
                        default:
                            console.log('%c%s', 'color: white; background: red; font-size: 12px;', '[STREAMDECK] websocket.onmessage +++++++++  PROBLEM ++++++++');
                            console.warn('UNREGISTERED MESSAGETYPE:', inMessageType);
                    }
                }

                if (m && m !== '') events.emit(m, jsonObj);
            };

            instance.connection = elgatoWebsocket;
        }

        return {
            // *** PUBLIC ***

            uuid: uuid,
            on: events.on,
            emit: events.emit,
            connection: elgatoWebsocket,
            connectElgatoWS: connectElgatoWS,
            api: null,
        };
    }

    return {
        getInstance: function () {
            if (!instance) {
                instance = init();
            }
            return instance;
        }
    };
})();

window.$SD = StreamDeckLink.getInstance();
window.$SD.api = SDApi;
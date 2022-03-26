import {PropertyInspector as SDInspector, Streamdeck} from "@rweich/streamdeck-ts";
import {FFXIVPluginLink} from "./link/ffxivplugin/FFXIVPluginLink";
import {DidReceiveGlobalSettingsEvent, DidReceiveSettingsEvent} from "@rweich/streamdeck-events/dist/Events/Received";
import {PIDispatcher} from "./inspector/PIDispatcher";
import {DefaultGlobalSettings, GlobalSettings} from "./util/GlobalSettings";
import {GlobalFrame} from "./inspector/frames/GlobalFrame";
import {FFXIVInitReply} from "./link/ffxivplugin/GameTypes";

class XIVDeckInspector {
    sdPluginLink: SDInspector = new Streamdeck().propertyinspector();
    xivPluginLink: FFXIVPluginLink = new FFXIVPluginLink(this.sdPluginLink);
    
    // state for this inspector
    uuid: string = "";
    dispatcher: PIDispatcher = new PIDispatcher();
    globalInspector: GlobalFrame = new GlobalFrame();
    private PIHelpers: any;
    
    constructor() {
        console.log(this);
        
        this.sdPluginLink.on('didReceiveGlobalSettings', (ev: DidReceiveGlobalSettingsEvent) => this.handleDidReceiveGlobalSettings(ev));
        this.sdPluginLink.on('didReceiveSettings', (ev: DidReceiveSettingsEvent) => this.handleDidReceiveSettings(ev))

        this.sdPluginLink.on('websocketOpen', (event => {
            this.dispatcher.initialize();
            
            this.uuid = event.uuid;
            this.sdPluginLink.getGlobalSettings(event.uuid);
            // this.sdPluginLink.getSettings(event.uuid);

            // load version into DOM (hacky)
            let pInfo = this.sdPluginLink.info.plugin as Record<string, string>;
            let rtVersion = document.getElementById('runtime-version');
            if (rtVersion) {
                rtVersion.innerText = pInfo.version;
            }
        }));
    }

    handleDidReceiveGlobalSettings(event: DidReceiveGlobalSettingsEvent) {
        let globalSettings = {...DefaultGlobalSettings, ...(event.settings as GlobalSettings)};
        this.globalInspector.loadSettings(globalSettings);
        
        this._initializeXIVLinkWS(globalSettings.ws.port)
    }
    
    handleDidReceiveSettings(event: DidReceiveSettingsEvent) {
        this.dispatcher.handleReceivedSettings(event);
    }
    
    private _initializeXIVLinkWS(port: number) {
        this.xivPluginLink.port = port;
        
        // The PI can't actually determine if the game is alive or not, so we'll force it to think it is.
        this.xivPluginLink.isGameAlive = true;
        
        this.xivPluginLink.on("_wsClosed", () => {
            let errElement = document.getElementById("errorDisplay")!;
            errElement.setAttribute("style", "");
            errElement.append(this.PIHelpers.generateConnectionErrorDom());
        });
        
        this.xivPluginLink.on("initReply", (data: FFXIVInitReply) => {
            let verElement = document.getElementById("xivdeck-game-version");
            if (verElement) {
                verElement.innerText = data.version;
            }
        });
        
        this.xivPluginLink.connect(false);
    }
    
}


const piInstance = new XIVDeckInspector();
export default piInstance;
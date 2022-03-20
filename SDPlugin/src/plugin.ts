import { FFXIVPluginLink } from "./link/ffxivplugin/FFXIVPluginLink"
import { Streamdeck, Plugin as SDPlugin } from "@rweich/streamdeck-ts";
import {DidReceiveGlobalSettingsEvent, DidReceiveSettingsEvent} from "@rweich/streamdeck-events/dist/Events/Received"
import {
    ApplicationDidLaunchEvent,
    ApplicationDidTerminateEvent, KeyDownEvent,
    WillAppearEvent, WillDisappearEvent
} from "@rweich/streamdeck-events/dist/Events/Received/Plugin"
import {GlobalSettings} from "./util/GlobalSettings";
import {ButtonDispatcher} from "./button/ButtonDispatcher";

class XIVDeckPlugin {
    sdPluginLink: SDPlugin = new Streamdeck().plugin();
    xivPluginLink: FFXIVPluginLink = new FFXIVPluginLink();
    
    private dispatcher: ButtonDispatcher = new ButtonDispatcher();
    
    constructor() {
        this.sdPluginLink.on('didReceiveGlobalSettings', (ev: DidReceiveGlobalSettingsEvent) => this.handleDidReceiveGlobalSettings(ev));
        
        this.sdPluginLink.on('applicationDidLaunch', (ev: ApplicationDidLaunchEvent) => this.handleApplicationDidLaunch(ev));
        this.sdPluginLink.on('applicationDidTerminate', (ev: ApplicationDidTerminateEvent) => this.handleApplicationDidTerminate(ev));
        
        // per-button dispatches
        this.sdPluginLink.on('willAppear', (ev: WillAppearEvent) => this.dispatcher.handleWillAppear(ev));
        this.sdPluginLink.on('willDisappear', (ev: WillDisappearEvent) => this.dispatcher.handleWillDisappear(ev));
        this.sdPluginLink.on('keyDown', (ev: KeyDownEvent) => this.dispatcher.handleKeyDown(ev));
        this.sdPluginLink.on('didReceiveSettings', (ev: DidReceiveSettingsEvent) => this.dispatcher.handleReceivedSettings(ev));

        this.sdPluginLink.on('websocketOpen', () => {
            this.sdPluginLink.getGlobalSettings(this.sdPluginLink.pluginUUID as string);
        });
    }

    handleDidReceiveGlobalSettings(event: DidReceiveGlobalSettingsEvent) {
        // if a connection is already running and the port changed, close and re-open it.
        let globalSettings = event.settings as GlobalSettings
        
        if (this.xivPluginLink.isReady() && this.xivPluginLink.port != globalSettings.ws.port) {
            this.xivPluginLink.port = globalSettings.ws.port;
            this.xivPluginLink.close();
            console.debug('tweaked port');
        }

        // it may be worth noting that we're pretty connect-happy here. we're relying on the connect() method to perform
        // the required pre-connection checks (a connection doesn't already exist, the game is alive, etc.) to prevent
        // us from spamming everything. This is an ugly solution, but the initialization logic we have to deal with is
        // also ugly.
        this.xivPluginLink.connect(true);
    }
    
    handleApplicationDidLaunch(event: ApplicationDidLaunchEvent) {
        // we can be dumb here, as the only time this event will ever be fired is when ffxiv_dx11.exe launches.
        this.xivPluginLink.isGameAlive = true;
        this.xivPluginLink.connect(true);
    }
    
    handleApplicationDidTerminate(event: ApplicationDidTerminateEvent) {
        // handle cleanup of the websocket -- it should have already closed due to the other side disappearing, but this
        // will force that just in case.
        this.xivPluginLink.isGameAlive = false;
        this.xivPluginLink.close();
    }
}

const plugin = new XIVDeckPlugin();

// @ts-ignore
window.sdPlugin = plugin;

export default plugin;
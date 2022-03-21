import { FFXIVPluginLink } from "./link/ffxivplugin/FFXIVPluginLink"
import { Streamdeck, Plugin as SDPlugin } from "@rweich/streamdeck-ts";
import {DidReceiveGlobalSettingsEvent, DidReceiveSettingsEvent} from "@rweich/streamdeck-events/dist/Events/Received"
import {
    ApplicationDidLaunchEvent,
    ApplicationDidTerminateEvent, KeyDownEvent,
    WillAppearEvent, WillDisappearEvent
} from "@rweich/streamdeck-events/dist/Events/Received/Plugin"
import {DefaultGlobalSettings, GlobalSettings} from "./util/GlobalSettings";
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
    }

    handleDidReceiveGlobalSettings(event: DidReceiveGlobalSettingsEvent) {
        let globalSettings = {...DefaultGlobalSettings, ...(event.settings as GlobalSettings)};
        this.xivPluginLink.port = globalSettings.ws.port;
        
        console.warn(this, event, globalSettings);
        
        // if a connection already exists and is running, close it and re-open it. otherwise, just start a new connect
        if (this.xivPluginLink.isReady()) {
            this.xivPluginLink.gracefulClose();
        } else {
            this.xivPluginLink.connect(true);
        }
    }
    
    handleApplicationDidLaunch(_: ApplicationDidLaunchEvent) {
        // on launch, mark the game as alive and reinitialize by calling global settings again
        this.xivPluginLink.isGameAlive = true;
        this.sdPluginLink.getGlobalSettings(this.sdPluginLink.pluginUUID!);
    }
    
    handleApplicationDidTerminate(_: ApplicationDidTerminateEvent) {
        // Mark the game as dead, and stop retries.
        this.xivPluginLink.isGameAlive = false;
    }
}

const plugin = new XIVDeckPlugin();

// @ts-ignore
window.sdPlugin = plugin;

export default plugin;
#nullable enable
using System;
using Dalamud.Configuration;

namespace XIVDeck.FFXIVPlugin.Base; 

[Serializable]
public class PluginConfig : IPluginConfiguration {
    public int Version { get; set; } = 0;

    /**
         * Disabling safe mode allows for certain verification checks that the plugin does to be skipped.
         * This value can *only* be set through manual configuration edits.
         */
    public bool SafeMode { get; set; } = true;

    /**
         * This boolean saves a persistent configuration for if a Stream Deck has ever been connected to the plugin.
         * If not, the plugin will display a nag message on startup to install and set up the associated plugin.
         */
    public bool HasLinkedStreamDeckPlugin { get; set; }

    /** 
         * EXPERIMENTAL - This boolean allows integration with the Penumbra IPC for icon load purposes.
         */
    public bool UsePenumbraIPC { get; set; }

    /**
         * EXPERIMENTAL - This boolean allows using /micon values for the Macro field
         */
    public bool UseMIconIcons { get; set; } = true;
        
    public int WebSocketPort { get; set; } = 37984;

    /// <summary>
    /// Configure XIVDeck to listen to all available interfaces, rather than just localhost.
    ///
    /// This setting is config file only, as it introduces security concerns.
    /// </summary>
    public bool ListenOnAllInterfaces { get; set; } = false;

    public void Save() {
        Injections.PluginInterface.SavePluginConfig(this);
    }
}
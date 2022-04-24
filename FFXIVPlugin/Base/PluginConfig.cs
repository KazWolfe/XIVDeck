#nullable enable
using System;
using Dalamud.Configuration;
using Dalamud.Plugin;

namespace XIVDeck.FFXIVPlugin.Base {
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
        
        public int WebSocketPort { get; set; } = 37984;

        [NonSerialized]
        private DalamudPluginInterface? _pluginInterface;

        public void Initialize(DalamudPluginInterface @interface) {
            this._pluginInterface = @interface;
        }

        public void Save() {
            this._pluginInterface!.SavePluginConfig(this);
        }
    }
}
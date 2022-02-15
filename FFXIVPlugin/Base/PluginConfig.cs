#nullable enable
using Dalamud.Configuration;
using Dalamud.Plugin;
using System;

namespace FFXIVPlugin.helpers {
    [Serializable]
    public class PluginConfig : IPluginConfiguration {
        public int Version { get; set; } = 0;

        public bool SomePropertyToBeSavedAndWithADefault { get; set; } = true;

        /**
         * Disabling safe mode allows for certain verification checks that the plugin does to be skipped.
         * This value can *only* be set through manual configuration edits.
         */
        public bool SafeMode { get; set; } = true;

        /**
         * This boolean saves a persistent configuration for if a Stream Deck has ever been connected to the plugin.
         * If not, the plugin will display a nag message on startup to install and set up the associated plugin.
         */
        public bool HasLinkedStreamDeckPlugin { get; set; } = false;

        public int WebSocketPort { get; set; } = 37984;

        // Authentication parameters. These are *usually* disabled, but exist just in case someone wants it.
        public bool RequireAuthentication { get; set; } = false;
        public string AuthenticationKey { get; set; } = "";

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
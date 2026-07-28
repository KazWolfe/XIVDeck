using System;
using System.Collections.Generic;
using Dalamud.Configuration;
using Newtonsoft.Json;

namespace XIVDeck.FFXIVPlugin.Config;

[Serializable]
public class PluginConfig : IPluginConfiguration {
    public int Version { get; set; } = ConfigService.CurrentVersion;

    /// <summary>
    /// Determines if the plugin is in "safe mode". Disabling this setting allows certain high-risk actions to be taken
    /// without any guards.
    /// </summary>
    public bool SafeMode { get; set; } = true;

    /// <summary>
    /// Set when a Stream Deck (or other API client) has been linked to this game plugin at least once. Effectively used
    /// to track setup state of the application.
    /// </summary>
    public bool HasLinkedStreamDeckPlugin { get; set; }

    /// <summary>
    /// Per-type transport settings, keyed by a transport config type (stringified).
    /// See <see cref="ActiveTransport"/> for the currently used transport.
    /// </summary>
    [JsonConverter(typeof(TransportConfigDictionaryConverter))]
    public Dictionary<TransportType, TransportConfig> TransportSettings { get; set; } = new();

    /// <summary>
    /// The current transport used by this plugin.
    /// </summary>
    public TransportType ActiveTransport { get; set; } = TransportPlatform.NativeDefault;

    /// <summary>
    /// Looks up the settings for a given transport Type, creating (and storing) a default instance
    /// if none has been configured yet.
    /// </summary>
    public TransportConfig GetTransport(TransportType type) {
        if (!this.TransportSettings.TryGetValue(type, out var config)) {
            config = type switch {
                TransportType.WebSocket => new WebSocketTransportConfig(),
                TransportType.UnixDomainSocket => new UnixSocketTransportConfig(),
                TransportType.NamedPipe => new NamedPipeTransportConfig(),
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, @"Unknown transport type."),
            };

            this.TransportSettings[type] = config;
        }

        return config;
    }
}

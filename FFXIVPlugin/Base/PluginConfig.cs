#nullable enable
using System;
using Dalamud.Configuration;
using EmbedIO;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace XIVDeck.FFXIVPlugin.Base;

[Serializable]
public class PluginConfig : IPluginConfiguration {
    public int Version { get; set; } = 0;
    
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
    /// Set when the user has requested the Penumbra IPC be enabled to show modded icons on the Stream Deck.
    /// </summary>
    public bool UsePenumbraIPC { get; set; }
    
    /// <summary>
    /// Set when the user has requested that macro icons set through the /micon command are resolved. When not set,
    /// XIVDeck will instead use the icon specified in the macro dropdown, regardless of /micon setting.
    /// </summary>
    public bool UseMIconIcons { get; set; } = true;

    /// <summary>
    /// The port that XIVDeck should listen to on localhost.
    /// </summary>
    public int WebSocketPort { get; set; } = 37984;

    /// <summary>
    /// Configure XIVDeck to listen to all available interfaces, rather than just localhost.
    ///
    /// This setting is config file only, as it introduces security concerns.
    /// </summary>
    public bool ListenOnAllInterfaces { get; set; } = false;

    /// <summary>
    /// Set the HTTP Listener Mode used by EmbedIO.
    ///
    /// Internal configuration setting for now.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
    public HttpListenerMode? HttpListenerMode { get; set; }

    /// <summary>
    /// Suppresses nag messages related to multiboxing.
    /// </summary>
    public bool SuppressMultiboxNag { get; set; } = false;

    public void Save() {
        Injections.PluginInterface.SavePluginConfig(this);
    }
}
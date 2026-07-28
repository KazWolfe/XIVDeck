using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using XIVDeck.FFXIVPlugin.RpcServer.Transports;

namespace XIVDeck.FFXIVPlugin.Config;

[JsonConverter(typeof(StringEnumConverter))]
public enum TransportType {
    WebSocket,
    UnixDomainSocket,
    NamedPipe,
}

public class TransportConfig;

[Serializable]
public class WebSocketTransportConfig : TransportConfig {
    /// <summary>
    /// Whether to listen on all interfaces (0.0.0.0 and ::/0) or just localhost.
    /// For future use (maybe?).
    /// </summary>
    public bool ListenToAllInterfaces { get; set; } = false;

    /// <summary>
    /// The port to listen to.
    /// </summary>
    public int Port { get; set; } = WebSocketServer.DefaultWebsocketPort;
}

public class UnixSocketTransportConfig : TransportConfig {
    /// <summary>
    /// The identifier to use for this transport. When set, this will be used as a static key for this configuration
    /// instead of an auto-generated identifier/name via the PID of this process. Not currently used.
    /// </summary>
    public string? ClientIdentifier;
}

public class NamedPipeTransportConfig : TransportConfig {
    /// <summary>
    /// The identifier to use for this transport. When set, this will be used as a static key for this configuration
    /// instead of an auto-generated identifier/name via the PID of this process. Not currently used.
    /// </summary>
    public string? ClientIdentifier;
}

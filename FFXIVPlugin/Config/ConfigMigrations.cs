using System;
using System.Collections.Generic;
using XIVDeck.FFXIVPlugin.Config.Versions;
using XIVDeck.FFXIVPlugin.RpcServer.Transports;

namespace XIVDeck.FFXIVPlugin.Config;

internal static class ConfigMigrations {
    public static readonly Dictionary<int, Type> VersionTypes = new() {
        [0] = typeof(PluginConfigV0),
    };

    private static readonly Dictionary<int, Func<object, object>> Steps = new() {
        [0] = raw => ToV1((PluginConfigV0)raw),
    };

    public static object Apply(int fromVersion, object config) {
        if (!Steps.TryGetValue(fromVersion, out var step)) {
            throw new InvalidOperationException(
                $"No migration registered to move plugin config from version {fromVersion} forward.");
        }

        return step(config);
    }

    private static PluginConfig ToV1(PluginConfigV0 v0) {
        var customizedWSPort = v0.WebSocketPort != WebSocketServer.DefaultWebsocketPort;

        return new PluginConfig {
            Version = 1,
            SafeMode = v0.SafeMode,
            HasLinkedStreamDeckPlugin = v0.HasLinkedStreamDeckPlugin,
            ActiveTransport = customizedWSPort ? TransportType.WebSocket : TransportPlatform.NativeDefault,
            TransportSettings = new Dictionary<TransportType, TransportConfig> {
                [TransportType.WebSocket] = new WebSocketTransportConfig { Port = v0.WebSocketPort },
            },
        };
    }
}

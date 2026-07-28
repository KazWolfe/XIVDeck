using System;
using XIVDeck.FFXIVPlugin.RpcServer.Transports;

namespace XIVDeck.FFXIVPlugin.Config.Versions;

[Serializable]
public class PluginConfigV0 {
    public int Version { get; set; } = 0;

    public bool SafeMode { get; set; } = true;

    public bool HasLinkedStreamDeckPlugin { get; set; }

    public bool UseMIconIcons { get; set; } = true;

    public int WebSocketPort { get; set; } = WebSocketServer.DefaultWebsocketPort;

    public bool SuppressMultiboxNag { get; set; } = false;
}

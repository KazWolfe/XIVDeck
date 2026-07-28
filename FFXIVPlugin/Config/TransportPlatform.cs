using System.Collections.Generic;
using Dalamud.Utility;

namespace XIVDeck.FFXIVPlugin.Config;

public static class TransportPlatform {
    public static readonly TransportType NativeDefault = Util.IsWine() ? TransportType.UnixDomainSocket : TransportType.NamedPipe;
    public static readonly IReadOnlyList<TransportType> AllowedTypes = [TransportType.WebSocket, NativeDefault];
}

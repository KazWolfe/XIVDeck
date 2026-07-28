using System;
using StreamJsonRpc;
using XIVDeck.FFXIVPlugin.Config;

namespace XIVDeck.FFXIVPlugin.RpcServer;

public interface IRpcTransport : IDisposable {
    public TransportType Type { get; }

    public void StartServer();
    public void StopServer();

    public bool IsRunning { get; }

    public event Action<IJsonRpcMessageHandler>? Connected;
}

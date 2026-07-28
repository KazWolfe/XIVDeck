using System;
using System.Collections.Concurrent;
using Autofac;
using Serilog;
using StreamJsonRpc;
using XIVDeck.FFXIVPlugin.Exceptions;
using XIVDeck.FFXIVPlugin.Game.Chat;
using XIVDeck.FFXIVPlugin.IoC;

namespace XIVDeck.FFXIVPlugin.RpcServer;

[Service(ServiceFlags.Singleton | ServiceFlags.AutoLoad)]
public class RpcManager : IDisposable {
    private readonly ILogger _log;
    private readonly ILifetimeScope _rootScope;
    private readonly TransportManager _transportManager;
    private readonly ErrorNotifier _errorNotifier;

    private readonly ConcurrentDictionary<Guid, RpcClient> _clients = new();
    public RpcManager(ILogger logger, ILifetimeScope scope, TransportManager transportManager, ErrorNotifier errorNotifier) {
        this._log = logger;
        this._rootScope = scope;
        this._transportManager = transportManager;
        this._errorNotifier = errorNotifier;

        transportManager.Connected += this.OnConnected;
    }

    private void OnConnected(IJsonRpcMessageHandler handler) {
        var client = new RpcClient(this._log.ForContext<RpcClient>(), handler, this._rootScope);

        this._clients[client.Id] = client;
        client.OnError += this.OnClientError;

        client.Disconnected += disconnected => {
             this._clients.TryRemove(disconnected.Id, out _);
             disconnected.OnError -= this.OnClientError;
        };
    }

    public void Dispose() {
        foreach (var client in this._clients.Values) {
            client.Dispose();
        }

        this._transportManager.Connected -= this.OnConnected;

        this._clients.Clear();

        GC.SuppressFinalize(this);
    }

    private void OnClientError(Exception ex) {
        if (ex is IXIVDeckException) {
            this._errorNotifier.ShowError(ex.Message, useToast: true, debounce: true);
        }
    }
}

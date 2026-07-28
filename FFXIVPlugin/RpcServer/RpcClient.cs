using System;
using System.Reflection;
using System.Threading.Tasks;
using Autofac;
using Serilog;
using StreamJsonRpc;
using XIVDeck.FFXIVPlugin.RpcServer.Helpers;
using XIVDeck.FFXIVPlugin.RpcServer.Services;

namespace XIVDeck.FFXIVPlugin.RpcServer;

public class RpcClient : IDisposable {
    public Guid Id { get; } = Guid.NewGuid();

    public event Action<RpcClient>? Disconnected;

    public event Action<Exception>? OnError;

    private readonly ILogger _log;
    private readonly XivDeckJsonRpc _rpc;
    private readonly ILifetimeScope _scope;

    public RpcClient(ILogger logger, IJsonRpcMessageHandler handler, ILifetimeScope rootScope) {
        this._log = logger;
        this._rpc = new XivDeckJsonRpc(handler);
        this._rpc.OnError += ex => this.OnError?.Invoke(ex);

        this._scope = rootScope.BeginLifetimeScope(builder => {
            builder.RegisterInstance(this).As<RpcClient>().ExternallyOwned();
            RpcServiceWiring.RegisterAssemblyServices(builder);
        });

        this.MountEssential();

        // necessary as we have to "unlock" the full API after the handshake completes
        this._rpc.AllowModificationWhileListening = true;

        this._rpc.Disconnected += (_, e) => {
            if (e.Reason is DisconnectedReason.RemotePartyTerminated or DisconnectedReason.LocallyDisposed) {
                this._log.Debug("JSON-RPC client {ClientId} disconnected: {Reason}", this.Id, e.Reason);
            } else {
                this._log.Warning(e.Exception,
                    "JSON-RPC client {ClientId} disconnected unexpectedly: {Reason} - {Description}",
                    this.Id, e.Reason, e.Description);
            }

            this.Disconnected?.Invoke(this);

            this._scope.Dispose();
        };

        // FIXME: architectural shenanigans.
        this._rpc.Completion.ContinueWith(
            static t => _ = t.Exception,
            TaskContinuationOptions.ExecuteSynchronously | TaskContinuationOptions.OnlyOnFaulted);

        this._rpc.StartListening();
        this._log.Debug("JSON-RPC client {ClientId} connected.", this.Id);
    }

    private void MountEssential() {
        var connectionService = this._scope.Resolve<ConnectionService>();
        var prefix = typeof(ConnectionService).GetCustomAttribute<RpcServiceAttribute>()!.Name;

        this.Mount(prefix, connectionService);
    }

    public void MountAll() {
        foreach (var (prefix, instance) in RpcServiceWiring.ResolveTargets(this._scope)) {
            if (instance is ConnectionService) continue;

            this.Mount(prefix, instance);
        }
    }

    private void Mount(string prefix, object instance) {
        this._rpc.AddLocalRpcTarget(instance, new JsonRpcTargetOptions {
            MethodNameTransform = name => $"{prefix}.{name}",
            EventNameTransform = name => $"{prefix}.{name}",
        });
    }

    /// <summary>
    /// Disconnect this client once the response to the current request has been sent.
    /// </summary>
    public void DisconnectAfterResponse() {
        this._rpc.DisconnectAfterResponse();
    }

    public void Dispose() {
        this._rpc.Dispose();

        this.Disconnected = null;
        this.OnError = null;

        GC.SuppressFinalize(this);
    }
}

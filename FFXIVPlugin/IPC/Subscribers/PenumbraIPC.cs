using System;
using Dalamud.Logging;
using Dalamud.Plugin.Ipc;
using Dalamud.Plugin.Ipc.Exceptions;
using XIVDeck.FFXIVPlugin.Base;

namespace XIVDeck.FFXIVPlugin.IPC.Subscribers;

[PluginIpc]
internal class PenumbraIPC : IPluginIpcClient {
    // note: this is *extremely fragile* and honestly bad, but this will be instantiated by the system at a higher
    // level. if we want to consume the IPC, this should be a safe-ish way to do it, assuming null checks are used.
    internal static PenumbraIPC? Instance;
    
    public bool Enabled { get; private set; }
    private ICallGateSubscriber<int>? _penumbraApiVersionSubscriber;
    private ICallGateSubscriber<string, string>? _penumbraResolveDefaultSubscriber;

    private readonly ICallGateSubscriber<object?> _penumbraRegisteredSubscriber;
    
    internal PenumbraIPC() {
        Instance = this;

        try {
            this._initializeIpc();
        } catch (Exception ex) {
            PluginLog.Warning(ex, "Failed to initialize Penumbra IPC");
        }

        this._penumbraRegisteredSubscriber = Injections.PluginInterface.GetIpcSubscriber<object?>("Penumbra.Initialized");
        this._penumbraRegisteredSubscriber.Subscribe(this._initializeIpc);
    }

    public void Dispose() {
        this._penumbraRegisteredSubscriber.Unsubscribe(this._initializeIpc);

        // explicitly reset to null so that any future calls fail gracefully
        this._penumbraApiVersionSubscriber = null;
        this._penumbraResolveDefaultSubscriber = null;

        this.Enabled = false;

        GC.SuppressFinalize(this);
    }

    public int Version => this._penumbraApiVersionSubscriber?.InvokeFunc() ?? -1;

   private void _initializeIpc() {
       if (!Injections.PluginInterface.PluginNames.Contains("Penumbra")) {
           PluginLog.Debug("Penumbra was not found, will not create IPC at this time");
           return;
       }
       
       this._penumbraApiVersionSubscriber = Injections.PluginInterface.GetIpcSubscriber<int>("Penumbra.ApiVersion");
       this._penumbraResolveDefaultSubscriber = Injections.PluginInterface.GetIpcSubscriber<string, string>("Penumbra.ResolveDefaultPath");

       this.Enabled = true;
   }
   
    public string ResolvePenumbraPath(string path) {
        if (!this.Enabled || this._penumbraResolveDefaultSubscriber == null) return path;

        try {
            return this._penumbraResolveDefaultSubscriber.InvokeFunc(path);
        } catch (IpcNotReadyError) {
            PluginLog.Debug("Got a NotReadyError trying to call ResolveDefaultPath. Falling back to normal path");
            return path;
        } catch (Exception ex) {
            PluginLog.Error(ex, "Failed to invoke Penumbra IPC, disabling!");
            this.Enabled = false;
            
            return path;
        }
    }
}
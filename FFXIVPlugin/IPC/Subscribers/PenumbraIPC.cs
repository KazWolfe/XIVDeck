using System;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Logging;
using Dalamud.Plugin.Ipc;
using XIVDeck.FFXIVPlugin.Base;
using XIVDeck.FFXIVPlugin.Game;
using XIVDeck.FFXIVPlugin.Resources.Localization;

namespace XIVDeck.FFXIVPlugin.IPC.Subscribers;

[PluginIpc]
internal class PenumbraIPC : IPluginIpcClient {
    private const int PenumbraIPCCompatVersion = 4;
    
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

        GC.SuppressFinalize(this);
    }

    public int Version => this._penumbraApiVersionSubscriber?.InvokeFunc() ?? 0;

   private void _initializeIpc() {
       if (!XIVDeckPlugin.Instance.PluginInterface.PluginNames.Contains("Penumbra")) {
           PluginLog.Debug("Penumbra was not found, will not create IPC at this time");
           return;
       }
       
       var versionEndpoint = Injections.PluginInterface.GetIpcSubscriber<int>("Penumbra.ApiVersion");

       // this line may explode with an exception, but that should be fine as we'd normally catch that.
       var version = versionEndpoint.InvokeFunc();
       
       this._penumbraApiVersionSubscriber = versionEndpoint;

       if (version == PenumbraIPCCompatVersion) {
           this._penumbraResolveDefaultSubscriber = Injections.PluginInterface.GetIpcSubscriber<string, string>("Penumbra.ResolveDefaultPath");
           
           this.Enabled = true;
           PluginLog.Information("Enabled Penumbra IPC connection!");
       } else {
           PluginLog.Warning($"Penumbra IPC detected, but version {version} is incompatible!");
           DeferredChat.SendOrDeferMessage(new SeStringBuilder()
               .AddUiForeground($"[{UIStrings.XIVDeck}] ", 514)
               .AddText(UIStrings.PenumbraIPC_VersionMismatch)
               .Build());
       }
   }
   
    public string ResolvePenumbraPath(string path) {
        return this._penumbraResolveDefaultSubscriber?.InvokeFunc(path) ?? path;
    }
}
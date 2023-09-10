﻿using System;
using System.Linq;

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
    public int Version { get; private set; } = -1;
    
    private ICallGateSubscriber<(int BreakingVersion, int Version)>? _penumbraApiVersionsSubscriber;
    private ICallGateSubscriber<string, string>? _penumbraResolveInterfaceSubscriber;

    private readonly ICallGateSubscriber<object?> _penumbraRegisteredSubscriber;

    internal PenumbraIPC() {
        Instance = this;
        
        // Register first and then try so that we don't hit a race
        this._penumbraRegisteredSubscriber = Injections.PluginInterface.GetIpcSubscriber<object?>("Penumbra.Initialized");
        this._penumbraRegisteredSubscriber.Subscribe(this._initializeIpc);
        
        try {
            this._initializeIpc();
        } catch (Exception ex) {
            Injections.PluginLog.Warning(ex, "Failed to initialize Penumbra IPC");
        }
    }

    public void Dispose() {
        this._penumbraRegisteredSubscriber.Unsubscribe(this._initializeIpc);

        // explicitly reset to null so that any future calls fail gracefully
        this._penumbraApiVersionsSubscriber = null;
        this._penumbraResolveInterfaceSubscriber = null;

        this.Enabled = false;

        GC.SuppressFinalize(this);
    }
    
   private void _initializeIpc() {
       if (Injections.PluginInterface.InstalledPlugins.All(p => p.InternalName != "Penumbra")) {
           Injections.PluginLog.Debug("Penumbra was not found, will not create IPC at this time");
           return;
       }
       
       this._penumbraApiVersionsSubscriber = Injections.PluginInterface.GetIpcSubscriber<(int, int)>("Penumbra.ApiVersions");
       this._penumbraResolveInterfaceSubscriber = Injections.PluginInterface.GetIpcSubscriber<string, string>("Penumbra.ResolveInterfacePath");

       try {
           (var breakingVersion, this.Version) = this._penumbraApiVersionsSubscriber.InvokeFunc();
           Injections.PluginLog.Debug($"Connected to Penumbra IPC, version {this.Version} (compat {breakingVersion}).");
       } catch (IpcNotReadyError ex) {
           Injections.PluginLog.Information(ex, "Penumbra was found but its IPC was not ready, will not enable IPC at this time");
           return;
       }

       this.Enabled = true;
   }
   
    public string ResolvePenumbraPath(string path) {
        if (!this.Enabled || this._penumbraResolveInterfaceSubscriber == null) return path;

        try {
            return this._penumbraResolveInterfaceSubscriber.InvokeFunc(path);
        } catch (IpcNotReadyError) {
            Injections.PluginLog.Debug("Got a NotReadyError trying to call ResolveInterfacePath. Falling back to normal path");
            return path;
        } catch (Exception ex) {
            Injections.PluginLog.Error(ex, "Failed to invoke Penumbra IPC, disabling!");
            this.Enabled = false;
            
            return path;
        }
    }
}
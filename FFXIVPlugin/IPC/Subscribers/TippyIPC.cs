using System;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Logging;
using Dalamud.Plugin.Ipc;
using XIVDeck.FFXIVPlugin.Base;

namespace XIVDeck.FFXIVPlugin.IPC.Subscribers; 

[PluginIpc]
internal class TippyIPC : IPluginIpcClient {
    public bool Enabled { get; private set; }
    
    private ICallGateSubscriber<int>? _tippyApiVersionSubscriber;
    private ICallGateSubscriber<string, bool>? _tippyRegisterTipSubscriber;

    private ICallGateSubscriber<bool> _tippyRegisteredSubscriber;

    internal TippyIPC() {
        this._tippyRegisteredSubscriber = Injections.PluginInterface.GetIpcSubscriber<bool>("Tippy.IsInitialized");
        this._tippyRegisteredSubscriber.Subscribe(this._initializeIpc);
        
        // n.b. we have a *very minor* race condition here where if Tippy initializes *after* the above subscribe but
        // before the initialize call below, we could do a double-init. 
        
        try {
            this._initializeIpc();
        } catch (Exception ex) {
            PluginLog.Warning(ex, "Failed to initialize Tippy IPC");
        }
    }

    public void Dispose() {
        this._tippyRegisteredSubscriber.Unsubscribe(this._initializeIpc);

        this._tippyApiVersionSubscriber = null;
        this._tippyRegisterTipSubscriber = null;
        
        GC.SuppressFinalize(this);
    }

    public int Version => this._tippyApiVersionSubscriber?.InvokeFunc() ?? 0;

    private void _initializeIpc() {
        if (!Injections.PluginInterface.PluginNames.Contains("Tippy")) {
            PluginLog.Debug("Tippy was not found, will not create IPC at this time");
            return;
        }
        
        var versionEndpoint = Injections.PluginInterface.GetIpcSubscriber<int>("Tippy.APIVersion");
        
        // this line may explode with an exception, but that should be fine as we'd normally catch that.
        var version = versionEndpoint.InvokeFunc();
       
        this._tippyApiVersionSubscriber = versionEndpoint;

        if (version == 1) {
            this._tippyRegisterTipSubscriber = Injections.PluginInterface.GetIpcSubscriber<string, bool>("Tippy.RegisterTip");
            this.Enabled = true;
            PluginLog.Information("Enabled Tippy IPC connection!");
            
            this.RegisterTips();
        } else if (version > 0) {
            PluginLog.Warning($"Tippy IPC detected, but version {version} is incompatible!");
        }
    }
    
    public bool RegisterTip(string tip) {
        return this._tippyRegisterTipSubscriber?.InvokeFunc(tip) ?? false;
    }

    private void RegisterTips() {
        var rng = new Random();
        var tips = new List<string> {
            "Why use both a keyboard and a Stream Deck? All your GCDs will feel right at home as Stream Deck buttons.",
            "Your Stream Deck is wired, so it'll always be faster than a wireless game controller!",
            "For a great FFXIV experience, buy the Stream Deck Pedal and bind a key to the /mdance emote.",
            "I see you have XIVDeck installed. You really shouldn't use plugins that will never leave testing.",
            "XIVDeck is not intended for roleplay. Memorize emote commands instead so you don't need to stop looking at chat.",
            "Connect your Stream Deck to your Steam Deck so you can have a great stream while you're outside on your deck.",
            "If you don't have a Stream Deck yet, be sure to buy one just for XIVDeck!",
            "Maximize your gaming potential by buying a Stream Deck XL. More buttons = more win!",
            "XIVDeck gives you a significant advantage in PVP by reducing button bloat. Use it often.",
            "If your Stream Deck icons aren't loading, make sure the game is running!"
        };

        tips.OrderBy(_ => rng.Next()).Take(3).ToList().ForEach(tip => this.RegisterTip(tip));
    }
}
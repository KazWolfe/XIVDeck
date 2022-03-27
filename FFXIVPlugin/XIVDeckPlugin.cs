﻿using System;
using Dalamud.Plugin;
using FFXIVClientStructs;
using XivCommon;
using XIVDeck.FFXIVPlugin.Base;
using XIVDeck.FFXIVPlugin.Game;
using XIVDeck.FFXIVPlugin.Server;
using XIVDeck.FFXIVPlugin.Server.Types;
using XIVDeck.FFXIVPlugin.UI;
using XIVDeck.FFXIVPlugin.UI.Windows;
using XIVDeck.FFXIVPlugin.UI.Windows.Nags;

namespace XIVDeck.FFXIVPlugin; 

// ReSharper disable once ClassNeverInstantiated.Global - instantiation handled by Dalamud
public sealed class XIVDeckPlugin : IDalamudPlugin {
    public static XIVDeckPlugin Instance = null!;
        
    public string Name => Constants.PluginName;
        
    public DalamudPluginInterface PluginInterface { get; init; }
    public PluginConfig Configuration { get; init; }
    public IconManager IconManager { get; set; }
    public PatchedWindowSystem WindowSystem;
    public XivCommonBase XivCommon { get; }
    public SigHelper SigHelper { get; }
        
    public GameStateCache GameStateCache { get; }

    private readonly HotbarWatcher _hotbarWatcher;
    private XIVDeckWebServer _xivDeckWebServer = null!;

    public XIVDeckPlugin(DalamudPluginInterface pluginInterface) {
        // Injections management
        pluginInterface.Create<Injections>();
        Resolver.Initialize(Injections.SigScanner.SearchBase);

        Instance = this;
            
        this.PluginInterface = pluginInterface;

        this.Configuration = this.PluginInterface.GetPluginConfig() as PluginConfig ?? new PluginConfig();
        this.Configuration.Initialize(this.PluginInterface);
            
        // Load caches of various things into plugin memory for reference later
        this.GameStateCache = GameStateCache.Load();
        SerializableGameClass.GetCache();

        // Various managers for advanced hooking into the game
        this.XivCommon = new XivCommonBase();
        this.SigHelper = new SigHelper();
        this.IconManager = new IconManager(this.PluginInterface);
        this._hotbarWatcher = new HotbarWatcher();
        this.WindowSystem = new PatchedWindowSystem(this.Name);

        // Start the websocket server itself.
        this.InitializeWebServer();
        
        this.PluginInterface.UiBuilder.Draw += this.WindowSystem.Draw;
        this.PluginInterface.UiBuilder.OpenConfigUi += this.DrawConfigUI;

        Injections.ClientState.Login += this.OnLogin;
        this.InitializeNag();
    }

    public void Dispose() {
        this.WindowSystem.RemoveAllWindows();

        this._hotbarWatcher.Dispose();
        this._xivDeckWebServer.Dispose();
        this.SigHelper.Dispose();

        Injections.ClientState.Login -= this.OnLogin;
    }

    internal void DrawConfigUI() {
        var instance = this.WindowSystem.GetWindow(SettingsWindow.WindowKey);
        
        if (instance == null) {
            this.WindowSystem.AddWindow(new SettingsWindow());
        }
    }

    private void OnLogin(object? _, EventArgs? __) {
        // game state isn't ready until login succeeds, so we wait for it to be ready before updating cache
        this.GameStateCache.Refresh();
    }

    private void InitializeNag() {
        if (!this.Configuration.HasLinkedStreamDeckPlugin) {
            SetupNag.Show();
        }
    }

    internal void InitializeWebServer() {
        if (this._xivDeckWebServer is {IsRunning: true}) {
            this._xivDeckWebServer.Dispose();
        }

        this._xivDeckWebServer = new XIVDeckWebServer(this.Configuration.WebSocketPort);
        this._xivDeckWebServer.StartServer();
    }
}
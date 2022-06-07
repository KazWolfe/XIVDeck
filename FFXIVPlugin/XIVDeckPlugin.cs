using System.Globalization;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using FFXIVClientStructs;
using XIVDeck.FFXIVPlugin.ActionExecutor;
using XIVDeck.FFXIVPlugin.Base;
using XIVDeck.FFXIVPlugin.Game;
using XIVDeck.FFXIVPlugin.IPC;
using XIVDeck.FFXIVPlugin.Resources.Localization;
using XIVDeck.FFXIVPlugin.Server;
using XIVDeck.FFXIVPlugin.Server.Types;
using XIVDeck.FFXIVPlugin.UI;
using XIVDeck.FFXIVPlugin.UI.Windows;
using XIVDeck.FFXIVPlugin.UI.Windows.Nags;

namespace XIVDeck.FFXIVPlugin; 

// ReSharper disable once ClassNeverInstantiated.Global - instantiation handled by Dalamud
public sealed class XIVDeckPlugin : IDalamudPlugin {
    internal static XIVDeckPlugin Instance = null!;
        
    public string Name => UIStrings.XIVDeck_Title;
        
    public DalamudPluginInterface PluginInterface { get; init; }
    public PluginConfig Configuration { get; init; }
    
    internal IconManager IconManager { get; }
    internal WindowSystem WindowSystem { get; }
    internal SigHelper SigHelper { get; }
    internal GameStateCache GameStateCache { get; }

    private readonly HotbarWatcher _hotbarWatcher;
    private XIVDeckWebServer _xivDeckWebServer = null!;
    private readonly ChatLinkWiring _chatLinkWiring;
    private readonly IPCManager _ipcManager;

    public XIVDeckPlugin(DalamudPluginInterface pluginInterface) {
        // Injections management
        pluginInterface.Create<Injections>();
        Resolver.Initialize(Injections.SigScanner.SearchBase);

        Instance = this;
            
        this.PluginInterface = pluginInterface;

        this.Configuration = this.PluginInterface.GetPluginConfig() as PluginConfig ?? new PluginConfig();
        this.Configuration.Initialize(this.PluginInterface);
        
        // Various managers for advanced hooking into the game
        this.IconManager = new IconManager();
        this.SigHelper = new SigHelper();
            
        // Load in and initialize a lot of various game state and plugin interface things.
        this.GameStateCache = GameStateCache.Load();
        SerializableGameClass.GetCache();
        ActionDispatcher.GetStrategies();
        this._ipcManager = new IPCManager();
        
        // More plugin interfaces
        this._chatLinkWiring = new ChatLinkWiring(this.PluginInterface);
        this._hotbarWatcher = new HotbarWatcher();
        this.WindowSystem = new WindowSystem(this.Name);
        
        // Start the websocket server itself.
        this.InitializeWebServer();
        
        this.PluginInterface.UiBuilder.Draw += this.WindowSystem.Draw;
        this.PluginInterface.UiBuilder.OpenConfigUi += this.DrawConfigUI;

        this.PluginInterface.LanguageChanged += this.UpdateLang;
        this.UpdateLang(this.PluginInterface.UiLanguage);

        Injections.ClientState.Login += DalamudHooks.OnGameLogin;
        this.InitializeNag();
    }

    public void Dispose() {
        this.WindowSystem.RemoveAllWindows();
        DeferredChat.Cancel();

        this._hotbarWatcher.Dispose();
        this._xivDeckWebServer.Dispose();
        this._chatLinkWiring.Dispose();
        this.SigHelper.Dispose();
        this._ipcManager.Dispose();

        Injections.ClientState.Login -= DalamudHooks.OnGameLogin;
        this.PluginInterface.LanguageChanged -= this.UpdateLang;
    }

    internal void DrawConfigUI() {
        var instance = this.WindowSystem.GetWindow(SettingsWindow.WindowKey);
        
        if (instance == null) {
            this.WindowSystem.AddWindow(new SettingsWindow());
        }
    }

    private void InitializeNag() {
        if (!this.Configuration.HasLinkedStreamDeckPlugin) {
            SetupNag.Show();
        }
    }

    private void UpdateLang(string langCode) {
        UIStrings.Culture = new CultureInfo(langCode);
    }

    internal void InitializeWebServer() {
        if (this._xivDeckWebServer is {IsRunning: true}) {
            this._xivDeckWebServer.Dispose();
        }

        this._xivDeckWebServer = new XIVDeckWebServer(this.Configuration.WebSocketPort);
        this._xivDeckWebServer.StartServer();
    }
}
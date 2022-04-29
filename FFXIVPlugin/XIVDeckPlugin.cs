using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using FFXIVClientStructs;
using XIVDeck.FFXIVPlugin.ActionExecutor;
using XIVDeck.FFXIVPlugin.Base;
using XIVDeck.FFXIVPlugin.Game;
using XIVDeck.FFXIVPlugin.IPC;
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
    public WindowSystem WindowSystem;
    public SigHelper SigHelper { get; }
        
    public GameStateCache GameStateCache { get; }

    private readonly HotbarWatcher _hotbarWatcher;
    private XIVDeckWebServer _xivDeckWebServer = null!;
    private readonly ChatLinkWiring _chatLinkWiring;
    private IPCManager _ipcManager;

    public XIVDeckPlugin(DalamudPluginInterface pluginInterface) {
        // Injections management
        pluginInterface.Create<Injections>();
        Resolver.Initialize(Injections.SigScanner.SearchBase);

        Instance = this;
            
        this.PluginInterface = pluginInterface;

        this.Configuration = this.PluginInterface.GetPluginConfig() as PluginConfig ?? new PluginConfig();
        this.Configuration.Initialize(this.PluginInterface);
            
        // Load in and initialize a lot of various game state and plugin interface things.
        this.GameStateCache = GameStateCache.Load();
        SerializableGameClass.GetCache();
        ActionDispatcher.GetStrategies();
        this._ipcManager = new IPCManager();

        // Various managers for advanced hooking into the game
        this.SigHelper = new SigHelper();
        this.IconManager = new IconManager(this.PluginInterface);
        this._chatLinkWiring = new ChatLinkWiring(this.PluginInterface);
        this._hotbarWatcher = new HotbarWatcher();
        this.WindowSystem = new WindowSystem(this.Name);
        
        // Start the websocket server itself.
        this.InitializeWebServer();
        
        this.PluginInterface.UiBuilder.Draw += this.WindowSystem.Draw;
        this.PluginInterface.UiBuilder.OpenConfigUi += this.DrawConfigUI;

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

    internal void InitializeWebServer() {
        if (this._xivDeckWebServer is {IsRunning: true}) {
            this._xivDeckWebServer.Dispose();
        }

        this._xivDeckWebServer = new XIVDeckWebServer(this.Configuration.WebSocketPort);
        this._xivDeckWebServer.StartServer();
    }
}
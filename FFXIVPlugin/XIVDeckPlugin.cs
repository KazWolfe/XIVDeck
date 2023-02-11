using System.Globalization;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using XIVDeck.FFXIVPlugin.ActionExecutor;
using XIVDeck.FFXIVPlugin.Base;
using XIVDeck.FFXIVPlugin.Game;
using XIVDeck.FFXIVPlugin.Game.Chat;
using XIVDeck.FFXIVPlugin.Game.Watchers;
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
    internal static XIVDeckPlugin Instance { get; private set; } = null!;
    public string Name => UIStrings.XIVDeck_Title;
        
    internal PluginConfig Configuration { get; }
    internal WindowSystem WindowSystem { get; }
    internal SigHelper SigHelper { get; }
    internal GameStateCache GameStateCache { get; }
    internal VolumeWatcher VolumeWatcher { get; }

    private DalamudPluginInterface PluginInterface { get; }
    private readonly HotbarWatcher _hotbarWatcher;
    private XIVDeckWebServer _xivDeckWebServer = null!;
    private readonly ChatLinkWiring _chatLinkWiring;
    private readonly IPCManager _ipcManager;

    public XIVDeckPlugin(DalamudPluginInterface pluginInterface) {
        pluginInterface.Create<Injections>();
        
        Instance = this;
        this.PluginInterface = pluginInterface;

        this.Configuration = this.PluginInterface.GetPluginConfig() as PluginConfig ?? new PluginConfig();
        
        // Various managers for advanced hooking into the game
        this.SigHelper = new SigHelper();

        // Load in and initialize a lot of various game state and plugin interface things.
        this.GameStateCache = new GameStateCache();
        SerializableGameClass.LoadCache();
        ActionDispatcher.GetStrategies();
        this._ipcManager = new IPCManager();
        
        // More plugin interfaces
        this._chatLinkWiring = new ChatLinkWiring();
        this._hotbarWatcher = new HotbarWatcher();
        this.VolumeWatcher = new VolumeWatcher();
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
        this.VolumeWatcher.Dispose();
        
        this._xivDeckWebServer.Dispose(); 
        this._chatLinkWiring.Dispose();
        this.SigHelper.Dispose();
        this._ipcManager.Dispose();

        Injections.ClientState.Login -= DalamudHooks.OnGameLogin;
        this.PluginInterface.LanguageChanged -= this.UpdateLang;

        // setting to null here is okay as this will only be called on plugin teardown.
        // Nothing should *ever* run past this point.
        Instance = null!;
    }

    internal void DrawConfigUI() {
        var window = SettingsWindow.GetOrCreate();
        window.Toggle();
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

        this._xivDeckWebServer = new XIVDeckWebServer(this);
        this._xivDeckWebServer.StartServer();
    }
}
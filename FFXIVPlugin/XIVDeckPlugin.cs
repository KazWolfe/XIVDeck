using System;
using Dalamud.Plugin;
using FFXIVClientStructs;
using XivCommon;
using XIVDeck.FFXIVPlugin.Base;
using XIVDeck.FFXIVPlugin.Game;
using XIVDeck.FFXIVPlugin.Server;
using XIVDeck.FFXIVPlugin.UI;
using XIVDeck.FFXIVPlugin.Utils;

namespace XIVDeck.FFXIVPlugin
{
    public sealed class XIVDeckPlugin : IDalamudPlugin {
        public static XIVDeckPlugin Instance = null!;
        
        public string Name => Constants.PluginName;
        
        public DalamudPluginInterface PluginInterface { get; init; }
        public PluginConfig Configuration { get; init; }
        public IconManager IconManager { get; set; }
        private PluginUI PluginUi { get; init; }
        public XivCommonBase XivCommon { get; }
        public SigHelper SigHelper { get; }
        
        public GameStateCache GameStateCache { get; }

        private HotbarWatcher HotbarWatcher;
        public XIVDeckWSServer XivDeckWsServer = null!;
        public XIVDeckWebServer XIVDeckWebServer = null!;

        public XIVDeckPlugin(DalamudPluginInterface pluginInterface) {
            // Injections management
            pluginInterface.Create<Injections>();
            Resolver.Initialize(Injections.SigScanner.SearchBase);

            Instance = this;
            
            this.PluginInterface = pluginInterface;

            this.Configuration = this.PluginInterface.GetPluginConfig() as PluginConfig ?? new PluginConfig();
            this.Configuration.Initialize(this.PluginInterface);
            
            // Load cache of unlocked events
            this.GameStateCache = GameStateCache.Load();

            // Various managers for advanced hooking into the game
            this.XivCommon = new XivCommonBase();
            this.SigHelper = new SigHelper();
            this.IconManager = new IconManager(this.PluginInterface);
            this.HotbarWatcher = new HotbarWatcher(this);
            
            // Start the websocket server itself.
            this.InitializeWSServer();
            this.InitializeWebServer();

            this.PluginUi = new PluginUI(this);

            this.PluginInterface.UiBuilder.Draw += this.DrawUI;
            this.PluginInterface.UiBuilder.OpenConfigUi += this.DrawConfigUI;

            Injections.ClientState.Login += this.OnLogin;
            this.InitializeNag();
        }

        public void Dispose() {
            this.PluginUi.Dispose();

            this.HotbarWatcher.Dispose();
            this.XivDeckWsServer.Dispose();
            this.XIVDeckWebServer.Dispose();
            this.SigHelper.Dispose();

            Injections.ClientState.Login -= this.OnLogin;
        }

        private void DrawUI() {
            this.PluginUi.Draw();
        }

        private void DrawConfigUI() {
            this.PluginUi.SettingsVisible = true;
        }

        private void OnLogin(object? _, EventArgs? __) {
            // game state isn't ready until login succeeds, so we wait for it to be ready before updating cache
            this.GameStateCache.Refresh();

            if (!this.Configuration.HasLinkedStreamDeckPlugin) {
                this.PluginUi.SettingsVisible = true;
            }
        }

        private void InitializeNag() {
            if (Injections.ClientState.IsLoggedIn && !this.Configuration.HasLinkedStreamDeckPlugin) {
                this.PluginUi.SettingsVisible = true;
            }
        }

        internal void InitializeWebServer() {
            if (this.XIVDeckWebServer is {IsRunning: true}) {
                this.XIVDeckWebServer.Dispose();
            }

            this.XIVDeckWebServer = new XIVDeckWebServer(this.Configuration.WebSocketPort + 1);
            this.XIVDeckWebServer.StartServer();
        }

        internal void InitializeWSServer() {
            // This is its own method as we may need to restart the websocket server later in the plugin lifecycle.
            // For example, if we change the server port, it doesn't make sense to restart the game.
            
            if (this.XivDeckWsServer is {IsStarted: true}) {
                this.XivDeckWsServer.Dispose();
            }

            this.XivDeckWsServer = new XIVDeckWSServer(this.Configuration.WebSocketPort);
            this.XivDeckWsServer.Start();
        }
    }
}
using Dalamud.Plugin;
using FFXIVClientStructs;
using FFXIVPlugin.Base;
using FFXIVPlugin.Server;
using FFXIVPlugin.ui;
using FFXIVPlugin.Utils;
using XivCommon;

namespace FFXIVPlugin
{
    public sealed class XIVDeckPlugin : IDalamudPlugin {
        public static XIVDeckPlugin Instance;
        
        public string Name => Constants.PluginName;
        
        public DalamudPluginInterface PluginInterface { get; init; }
        public PluginConfig Configuration { get; init; }
        public IconManager IconManager { get; set; }
        private PluginUI PluginUi { get; init; }
        public XivCommonBase XivCommon { get; }
        public SigHelper SigHelper { get; }
        
        public GameStateCache GameStateCache { get; }

        private HotbarWatcher HotbarWatcher;
        public XIVDeckWSServer XivDeckWsServer;

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
            this.GameStateCache.Refresh();

            // Various managers for advanced hooking into the game
            this.XivCommon = new XivCommonBase();
            this.SigHelper = new SigHelper();
            this.IconManager = new IconManager(this.PluginInterface);
            this.HotbarWatcher = new HotbarWatcher(this);
            
            // Start the websocket server itself.
            this.InitializeWSServer();

            this.PluginUi = new PluginUI(this);

            this.PluginInterface.UiBuilder.Draw += DrawUI;
            this.PluginInterface.UiBuilder.OpenConfigUi += DrawConfigUI;
        }

        public void Dispose() {
            this.PluginUi.Dispose();

            this.HotbarWatcher.Dispose();
            this.XivDeckWsServer.Dispose();
            this.SigHelper.Dispose();
        }

        private void DrawUI() {
            this.PluginUi.Draw();
        }

        private void DrawConfigUI() {
            this.PluginUi.SettingsVisible = true;
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
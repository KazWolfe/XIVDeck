using System.Drawing;
using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using System.IO;
using System.Reflection;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVPlugin.Base;
using FFXIVPlugin.helpers;
using FFXIVPlugin.Server;
using FFXIVPlugin.Utils;
using XivCommon;

namespace FFXIVPlugin
{
    public sealed class XIVDeckPlugin : IDalamudPlugin {
        public static XIVDeckPlugin Instance;
        
        public string Name => "XIVDeck Game Plugin";

        private const string commandName = "/xivdeckdebug";

        public DalamudPluginInterface PluginInterface { get; init; }
        public helpers.PluginConfig Configuration { get; init; }
        public IconManager IconManager { get; set; }
        private ui.PluginUI PluginUi { get; init; }
        public XivCommonBase XivCommon { get; }
        public SigHelper SigHelper { get; }
        
        public GameStateCache GameStateCache { get; }

        private HotbarWatcher HotbarWatcher;
        public XivDeckWSServer XivDeckWsServer;

        public XIVDeckPlugin(DalamudPluginInterface pluginInterface) {
            // Injections management
            pluginInterface.Create<Injections>();
            FFXIVClientStructs.Resolver.Initialize(Injections.SigScanner.SearchBase);

            Instance = this;
            
            this.PluginInterface = pluginInterface;

            this.Configuration = this.PluginInterface.GetPluginConfig() as helpers.PluginConfig ?? new helpers.PluginConfig();
            this.Configuration.Initialize(this.PluginInterface);

            // Various managers for advanced hooking into the game
            this.XivCommon = new XivCommonBase(Hooks.None);
            this.SigHelper = new SigHelper(Injections.SigScanner);
            this.IconManager = new IconManager(this.PluginInterface);
            this.HotbarWatcher = new HotbarWatcher(this);
            
            // Load cache of unlocked events
            this.GameStateCache = GameStateCache.Load();

            // And the WS server itself, though this should probably be converted to ASP.net or a different library
            this.XivDeckWsServer = new XivDeckWSServer(this.Configuration.WebSocketPort);
            this.XivDeckWsServer.Start();

            this.PluginUi = new ui.PluginUI(this);
            
            if (this.PluginInterface.IsDev) {
                Injections.CommandManager.AddHandler(commandName, new CommandInfo(OnCommand) {
                    HelpMessage = "A useful message to display in /xlhelp"
                });
            }

            this.PluginInterface.UiBuilder.Draw += DrawUI;
            this.PluginInterface.UiBuilder.OpenConfigUi += DrawConfigUI;
        }

        public void Dispose() {
            this.PluginUi.Dispose();

            if (this.PluginInterface.IsDev) {
                Injections.CommandManager.RemoveHandler(commandName);
            }

            this.HotbarWatcher.Dispose();
            this.XivDeckWsServer.Stop();
        }

        private void OnCommand(string command, string args) {
            // in response to the slash command, just display our main ui
            this.PluginUi.Visible = true;
        }

        private void DrawUI() {
            this.PluginUi.Draw();
        }

        private void DrawConfigUI() {
            this.PluginUi.SettingsVisible = true;
        }
    }
}
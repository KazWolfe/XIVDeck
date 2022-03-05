using Dalamud.Game;
using Dalamud.Logging;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using Newtonsoft.Json;
using XIVDeck.FFXIVPlugin.Base;
using XIVDeck.FFXIVPlugin.Server.Messages.Outbound;

namespace XIVDeck.FFXIVPlugin.Utils {
    public class HotbarWatcher {
        private XIVDeckPlugin _plugin;
        private HotBarSlot[,] _hotbarCache = new HotBarSlot[17,16];

        public HotbarWatcher(XIVDeckPlugin plugin) {
            Injections.Framework.Update += this.OnGameUpdate;
            this._plugin = plugin;
        }

        private unsafe void OnGameUpdate(Framework framework) {
            var hotbarModule =
                FFXIVClientStructs.FFXIV.Client.System.Framework.Framework.Instance()->GetUiModule()->
                    GetRaptureHotbarModule();

            var hotbarUpdated = false;
            
            for (var hotbarId = 0; hotbarId < 17; hotbarId++) {
                var hotbar = hotbarModule->HotBar[hotbarId];
                
                for (var slotId = 0; slotId < 16; slotId++) {
                    var gameSlot = hotbar->Slot[slotId];
                    var cachedSlot = this._hotbarCache[hotbarId, slotId];

                    if (gameSlot->CommandId == cachedSlot.CommandId && 
                        gameSlot->Icon == cachedSlot.Icon &&
                        gameSlot->CommandType == cachedSlot.CommandType) continue;
                    
                    hotbarUpdated = true;
                    this._hotbarCache[hotbarId, slotId] = new HotBarSlot {
                            CommandId = gameSlot->CommandId,
                            Icon = gameSlot->Icon,
                            CommandType = gameSlot->CommandType
                    };
                }
            }
            

            if (hotbarUpdated) {
                PluginLog.Debug("Detected a change to hotbar(s)!");

                var wsServer = this._plugin.XivDeckWsServer;
                wsServer.MulticastText(JsonConvert.SerializeObject(new WSHotbarUpdateMessage(this._hotbarCache)));
            }
        }

        public void Dispose() {
            Injections.Framework.Update -= this.OnGameUpdate;
        }
    }
}
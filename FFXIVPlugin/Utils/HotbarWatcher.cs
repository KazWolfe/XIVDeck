using System;
using Dalamud.Game;
using Dalamud.Logging;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using Newtonsoft.Json;
using XIVDeck.FFXIVPlugin.Base;
using XIVDeck.FFXIVPlugin.Server;
using XIVDeck.FFXIVPlugin.Server.Messages.Outbound;

namespace XIVDeck.FFXIVPlugin.Utils {
    public class HotbarWatcher : IDisposable {
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

                    // while we could put this refresh into the getIcon call instead, that will cause a few headaches:
                    // 1. Any hotbarIcon call that grabs an icon will *also* trigger a refresh call on the next framework tick.
                    // 2. A hidden slot icon updating won't actually trigger an update until the icon refreshes otherwise.
                    //
                    // This will tamper with game memory in a less-than-desirable way, but this should still be relatively safe
                    // as this method (loading from Slot B) is done by the game itself too. This *may* cause a problem with
                    // other plugins though, maybe?
                    if (gameSlot->Icon == 0 && gameSlot->CommandType != HotbarSlotType.Empty) {
                        XIVDeckPlugin.Instance.SigHelper.RefreshHotbarSlotIcon(gameSlot);
                    }
                    
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
                XIVDeckWSServer.Instance?.BroadcastMessage(new WSStateUpdateMessage("Hotbar"));
            }
        }

        public void Dispose() {
            Injections.Framework.Update -= this.OnGameUpdate;
            
            GC.SuppressFinalize(this);
        }
    }
}
using Dalamud.Game;
using Dalamud.Logging;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using FFXIVPlugin.Base;
using FFXIVPlugin.Server.Messages.Outbound;
using Newtonsoft.Json;

namespace FFXIVPlugin.Utils {
    public class HotbarWatcher {
        private XIVDeckPlugin _plugin;
        private HotBars _hotbarCache;
        
        public HotbarWatcher(XIVDeckPlugin plugin) {
            Injections.Framework.Update += this.OnGameUpdate;
            this._plugin = plugin;
        }

        public unsafe void OnGameUpdate(Framework framework) {
            var hotbarModule = FFXIVClientStructs.FFXIV.Client.System.Framework.Framework.Instance()->GetUiModule()->GetRaptureHotbarModule();

            if (this.checkHotbarEquality(hotbarModule->HotBar, this._hotbarCache)) {
                // no-op
            } else {
                PluginLog.Debug("Detected a change to hotbar(s)!");
                this._hotbarCache = hotbarModule->HotBar;

                var wsServer = this._plugin.XivDeckWsServer;
                wsServer.MulticastText(JsonConvert.SerializeObject(new WSHotbarUpdateMessage(this._hotbarCache)));
            }
        }

        private unsafe bool checkHotbarEquality(HotBars left, HotBars right) {
            for (int i = 0; i < 17; i++) {
                var hotbar = left[i];
                for (int j = 0; j < 16; j++) {
                    var slot = hotbar->Slot[j];
                    
                    if (slot->CommandId != right[i]->Slot[j]->CommandId) {
                        return false;
                    }
                }
            }

            return true;
        }

        public void Dispose() {
            Injections.Framework.Update -= this.OnGameUpdate;
        }
    }
}
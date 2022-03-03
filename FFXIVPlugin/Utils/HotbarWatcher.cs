using Dalamud.Game;
using Dalamud.Logging;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using Newtonsoft.Json;
using XIVDeck.FFXIVPlugin.Base;
using XIVDeck.FFXIVPlugin.Server.Messages.Outbound;

namespace XIVDeck.FFXIVPlugin.Utils {
    public class HotbarWatcher {
        private XIVDeckPlugin _plugin;
        private HotBars _hotbarCache;

        public HotbarWatcher(XIVDeckPlugin plugin) {
            Injections.Framework.Update += this.OnGameUpdate;
            this._plugin = plugin;
        }

        public unsafe void OnGameUpdate(Framework framework) {
            var hotbarModule =
                FFXIVClientStructs.FFXIV.Client.System.Framework.Framework.Instance()->GetUiModule()->
                    GetRaptureHotbarModule();

            if (this.CheckHotbarEquality(hotbarModule->HotBar, this._hotbarCache)) {
                // no-op
            }
            else {
                PluginLog.Debug("Detected a change to hotbar(s)!");
                this._hotbarCache = hotbarModule->HotBar;

                var wsServer = this._plugin.XivDeckWsServer;
                wsServer.MulticastText(JsonConvert.SerializeObject(new WSHotbarUpdateMessage(this._hotbarCache)));
            }
        }

        private unsafe bool CheckHotbarEquality(HotBars left, HotBars right) {
            for (int i = 0; i < 17; i++) {
                var hotbar = left[i];
                for (int j = 0; j < 16; j++) {
                    var leftSlot = hotbar->Slot[j];
                    var rightSlot = right[i]->Slot[j];

                    if (leftSlot->CommandId != rightSlot->CommandId || leftSlot->Icon != rightSlot->Icon ||
                        leftSlot->CommandType != rightSlot->CommandType) {
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
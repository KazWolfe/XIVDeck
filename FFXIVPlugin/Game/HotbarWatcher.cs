using System;
using Dalamud.Game;
using Dalamud.Logging;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using XIVDeck.FFXIVPlugin.Base;
using XIVDeck.FFXIVPlugin.Server;
using XIVDeck.FFXIVPlugin.Server.Messages.Outbound;

namespace XIVDeck.FFXIVPlugin.Game; 

public class HotbarWatcher : IDisposable {
    private readonly HotBarSlot[,] _hotbarCache = new HotBarSlot[17,16];

    public HotbarWatcher() {
        Injections.Framework.Update += this.OnGameUpdate;
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

                // ToDo: Maybe performance optimizations here by caching SlotB information instead?
                //       Would also involve making HotbarController read icon IDs from this cache, which might be
                //       a good idea anyways. To my knowledge, IconB will *always* change with Icon, so this might be
                //       the way to go.
                var calculatedIcon = XIVDeckPlugin.Instance.SigHelper.CalcIconForSlot(gameSlot);

                if (gameSlot->CommandId == cachedSlot.CommandId && 
                    calculatedIcon == cachedSlot.Icon &&
                    gameSlot->CommandType == cachedSlot.CommandType) continue;

                hotbarUpdated = true;
                this._hotbarCache[hotbarId, slotId] = new HotBarSlot {
                    CommandId = gameSlot->CommandId,
                    Icon = calculatedIcon,
                    CommandType = gameSlot->CommandType,
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
using System;
using System.Collections.Generic;
using Dalamud.Game;
using Dalamud.Logging;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using XIVDeck.FFXIVPlugin.Base;
using XIVDeck.FFXIVPlugin.Game.Managers;
using XIVDeck.FFXIVPlugin.Server;
using XIVDeck.FFXIVPlugin.Server.Messages.Outbound;
using XIVDeck.FFXIVPlugin.Server.Types;

namespace XIVDeck.FFXIVPlugin.Game.Watchers; 

public class HotbarWatcher : IDisposable {
    private readonly HotBarSlot[,] _hotbarCache = new HotBarSlot[17,16];

    public HotbarWatcher() {
        Injections.Framework.Update += this.OnGameUpdate;
    }

    private unsafe void OnGameUpdate(Framework framework) {
        var hotbarModule =
            FFXIVClientStructs.FFXIV.Client.System.Framework.Framework.Instance()->GetUiModule()->
                GetRaptureHotbarModule();
        
        List<MicroHotbarSlot> updatedSlots = new();

        for (var hotbarId = 0; hotbarId < 17; hotbarId++) {
            var hotbar = hotbarModule->HotBar[hotbarId];
                
            for (var slotId = 0; slotId < 16; slotId++) {
                var gameSlot = hotbar->Slot[slotId];
                var cachedSlot = this._hotbarCache[hotbarId, slotId];

                // ToDo: Maybe performance optimizations here by caching SlotB information instead?
                //       Would also involve making HotbarController read icon IDs from this cache, which might be
                //       a good idea anyways. To my knowledge, IconB will *always* change with Icon, so this might be
                //       the way to go.
                var calculatedIcon = HotbarManager.CalcIconForSlot(gameSlot);

                if (gameSlot->CommandId == cachedSlot.CommandId && 
                    calculatedIcon == cachedSlot.Icon &&
                    gameSlot->CommandType == cachedSlot.CommandType) continue;
                
                PluginLog.Verbose($"HOTBAR CHANGE ON HOTBAR {hotbarId}, SLOT {slotId}\n" +
                                  $"CommandID - Old: {cachedSlot.CommandId}  New: {gameSlot->CommandId}\n" +
                                  $"CommandType - Old: {cachedSlot.CommandType}  New: {gameSlot->CommandType}\n" +
                                  $"Icon - Old: {cachedSlot.Icon}  New: {calculatedIcon}");

                updatedSlots.Add(new MicroHotbarSlot(hotbarId, slotId));
                this._hotbarCache[hotbarId, slotId] = new HotBarSlot {
                    CommandId = gameSlot->CommandId,
                    Icon = calculatedIcon,
                    CommandType = gameSlot->CommandType
                };
            }
        }
            
        if (updatedSlots.Count > 0) {
            PluginLog.Debug("Detected a change to hotbar(s)!");
            var message = new WSStateUpdateMessage<List<MicroHotbarSlot>>("Hotbar", updatedSlots);
            XIVDeckWSServer.Instance?.BroadcastMessage(message);
        }
    }

    public void Dispose() {
        Injections.Framework.Update -= this.OnGameUpdate;
            
        GC.SuppressFinalize(this);
    }
}
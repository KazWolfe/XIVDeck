using System;
using System.Collections.Generic;
using Dalamud.Game;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using XIVDeck.FFXIVPlugin.Base;
using XIVDeck.FFXIVPlugin.Game.Managers;
using XIVDeck.FFXIVPlugin.Server.Messages.Outbound;
using XIVDeck.FFXIVPlugin.Server.Types;

namespace XIVDeck.FFXIVPlugin.Game.Watchers; 

public class HotbarWatcher : IDisposable {
    private readonly HotBarSlot[,] _hotbarCache = new HotBarSlot[17,16];

    public HotbarWatcher() {
        Injections.Framework.Update += this.OnGameUpdate;
    }

    private unsafe void OnGameUpdate(IFramework framework) {
        var hotbarModule =
            FFXIVClientStructs.FFXIV.Client.System.Framework.Framework.Instance()->GetUiModule()->
                GetRaptureHotbarModule();
        
        List<MicroHotbarSlot> updatedSlots = new();

        for (var hotbarId = 0; hotbarId < 17; hotbarId++) {
            var hotbar = hotbarModule->HotBar[hotbarId];
                
            for (var slotId = 0; slotId < 16; slotId++) {
                var gameSlot = hotbar->Slot[slotId];
                var cachedSlot = this._hotbarCache[hotbarId, slotId];

                // We calculate IconB first so that we know what "appearance" the slot has. This allows us to optimize
                // icon lookups, as they're the more expensive of the two calls. If IconB hasn't changed, the icon 
                // itself wouldn't have changed either.
                HotbarManager.CalcBForSlot(gameSlot, out var calculatedBType, out var calculatedBId);

                if (gameSlot->CommandId == cachedSlot.CommandId &&
                    gameSlot->CommandType == cachedSlot.CommandType &&
                    calculatedBType == cachedSlot.IconTypeB &&
                    calculatedBId == cachedSlot.IconB) continue;
                
                var calculatedIcon = gameSlot->GetIconIdForSlot(calculatedBType, calculatedBId);
                if (calculatedIcon == cachedSlot.Icon) continue;

                updatedSlots.Add(new MicroHotbarSlot(hotbarId, slotId));
                this._hotbarCache[hotbarId, slotId] = new HotBarSlot {
                    CommandId = gameSlot->CommandId,
                    Icon = calculatedIcon,
                    CommandType = gameSlot->CommandType,
                    IconTypeB = calculatedBType,
                    IconB = calculatedBId
                };
            }
        }
            
        if (updatedSlots.Count > 0) {
            Injections.PluginLog.Debug("Detected a change to hotbar(s)!");
            var message = new WSStateUpdateMessage<List<MicroHotbarSlot>>("Hotbar", updatedSlots);
            XIVDeckPlugin.Instance.Server.BroadcastMessage(message);
        }
    }

    public void Dispose() {
        Injections.Framework.Update -= this.OnGameUpdate;
            
        GC.SuppressFinalize(this);
    }
}
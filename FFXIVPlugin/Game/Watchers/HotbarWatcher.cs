using System;
using System.Collections.Generic;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using static FFXIVClientStructs.FFXIV.Client.UI.Misc.RaptureHotbarModule;
using XIVDeck.FFXIVPlugin.Base;
using XIVDeck.FFXIVPlugin.Game.Managers;
using XIVDeck.FFXIVPlugin.Server.Messages.Outbound;
using XIVDeck.FFXIVPlugin.Server.Types;

namespace XIVDeck.FFXIVPlugin.Game.Watchers;

public class HotbarWatcher : IDisposable {
    private readonly HotbarSlot[,] _hotbarCache = new HotbarSlot[17,16];

    public HotbarWatcher() {
        Injections.Framework.Update += this.OnGameUpdate;
    }

    private unsafe void OnGameUpdate(IFramework framework) {
        var hotbarModule =
            Framework.Instance()->GetUIModule()->
                GetRaptureHotbarModule();

        List<MicroHotbarSlot> updatedSlots = new();

        for (var hotbarId = 0; hotbarId < 17; hotbarId++) {
            ref var hotbar = ref hotbarModule->Hotbars[hotbarId];

            for (var slotId = 0; slotId < 16; slotId++) {
                var gameSlot = hotbar.GetHotbarSlot((uint) slotId);
                var cachedSlot = this._hotbarCache[hotbarId, slotId];

                // We calculate IconB first so that we know what "appearance" the slot has. This allows us to optimize
                // icon lookups, as they're the more expensive of the two calls. If IconB hasn't changed, the icon
                // itself wouldn't have changed either.
                HotbarManager.CalcBForSlot(gameSlot, out var calcApparentType, out var calcApparentId);

                if (gameSlot->CommandId == cachedSlot.CommandId &&
                    gameSlot->CommandType == cachedSlot.CommandType &&
                    calcApparentType == cachedSlot.ApparentSlotType &&
                    calcApparentId == cachedSlot.ApparentActionId) continue;

                var calculatedIcon = (uint)gameSlot->GetIconIdForSlot(calcApparentType, calcApparentId);
                if (calculatedIcon == cachedSlot.IconId) continue;

                updatedSlots.Add(new MicroHotbarSlot(hotbarId, slotId));
                this._hotbarCache[hotbarId, slotId] = new HotbarSlot {
                    CommandId = gameSlot->CommandId,
                    IconId = calculatedIcon,
                    CommandType = gameSlot->CommandType,
                    ApparentSlotType = calcApparentType,
                    ApparentActionId = calcApparentId
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

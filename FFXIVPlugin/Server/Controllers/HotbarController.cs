using System;
using Dalamud.Logging;
using EmbedIO;
using EmbedIO.Routing;
using EmbedIO.WebApi;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using XIVDeck.FFXIVPlugin.Base;
using XIVDeck.FFXIVPlugin.Game;
using XIVDeck.FFXIVPlugin.Server.Helpers;
using XIVDeck.FFXIVPlugin.Server.Types;

namespace XIVDeck.FFXIVPlugin.Server.Controllers;

[ApiController("/hotbar")]
public class HotbarController : WebApiController {
    [Route(HttpVerbs.Get, "/{hotbarId}/{slotId}")]
    public unsafe SerializableHotbarSlot GetHotbarSlot(int hotbarId, int slotId) {
        var plugin = XIVDeckPlugin.Instance;
        var hotbarModule = Framework.Instance()->GetUiModule()->GetRaptureHotbarModule();

        try {
            this.SafetyCheckHotbar(hotbarId, slotId);
        } catch (ArgumentException ex) {
            throw HttpException.BadRequest(ex.Message);
        }

        var hotbarItem = hotbarModule->HotBar[hotbarId]->Slot[slotId];
        var iconId = hotbarItem->Icon;

        return new SerializableHotbarSlot {
            HotbarId = hotbarId,
            SlotId = slotId,
            IconId = iconId,
            SlotType = hotbarItem->CommandType,
            CommandId = (int) hotbarItem->CommandId,
            IconData = plugin.IconManager.GetIconAsPngString(iconId % 1000000, iconId >= 1000000)
        };
        
    }
    
    [Route(HttpVerbs.Post, "/{hotbarId}/{slotId}/execute")]
    public unsafe void TriggerHotbarSlot(int hotbarId, int slotId) {
        PluginLog.Debug("timing: TriggerHotbarSlot started");
        var plugin = XIVDeckPlugin.Instance;
        var hotbarModule = Framework.Instance()->GetUiModule()->GetRaptureHotbarModule();

        try {
            this.SafetyCheckHotbar(hotbarId, slotId);
        } catch (ArgumentException ex) { 
            throw HttpException.BadRequest("An invalid hotbar or slot was triggered.", ex);
        }
        
        // this really should not be here as it's mixing the controller with business logic, but it can't really be
        // put anywhere else either.
        if (!Injections.ClientState.IsLoggedIn)
            throw HttpException.BadRequest("A player is not logged in to the game.");

        // Trigger the hotbar event on the next Framework tick, and also in the Framework (game main) thread.
        // For whatever reason, the game *really* doesn't like when a user casts a Weaponskill or Ability from a
        // non-game thread (as would be the case for API calls). Why this works normally for Spells and other
        // actions will forever be a mystery. 
        PluginLog.Debug("timing: pre-TickScheduler");
        TickScheduler.Schedule(delegate { 
            PluginLog.Debug("timing: TickScheduler start");
            var hotbarItem = hotbarModule->HotBar[hotbarId]->Slot[slotId];
            plugin.SigHelper.ExecuteHotbarSlot(hotbarItem);
            PluginLog.Debug("timing: TickScheduler end");
        });
        PluginLog.Debug("timing: TriggerHotbarSlot ended");
    }

    private void SafetyCheckHotbar(int hotbarId, int slotId) {
        switch (hotbarId) {
            // Safety checks
            case < 0 or > 17:
                throw new ArgumentException("Hotbar ID must be between 0 and 17");
            case < 10 when slotId is < 0 or > 11: // normal hotbars
                throw new ArgumentException("When hotbarID < 10, Slot ID must be between 0 and 11");
            case >= 10 when slotId is < 0 or > 15: // cross hotbars
                throw new ArgumentException("When Hotbar ID >= 10, Slot ID must be between 0 and 15");
        }
    }
}

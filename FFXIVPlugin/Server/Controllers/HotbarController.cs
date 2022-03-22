using System;
using EmbedIO;
using EmbedIO.Routing;
using EmbedIO.WebApi;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using XIVDeck.FFXIVPlugin.Base;
using XIVDeck.FFXIVPlugin.Server.Helpers;
using XIVDeck.FFXIVPlugin.Server.Types;
using XIVDeck.FFXIVPlugin.Utils;

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
            throw HttpException.NotAcceptable(ex.Message);
        }

        var hotbarItem = hotbarModule->HotBar[hotbarId]->Slot[slotId];
        var iconId = hotbarItem->Icon;
        
        String pngString = plugin.IconManager.GetIconAsPngString(iconId % 1000000, iconId >= 1000000);

        return new SerializableHotbarSlot {
            HotbarId = hotbarId,
            SlotId = slotId,
            IconId = iconId,
            SlotType = hotbarItem->CommandType,
            IconData = pngString
        };
        
    }
    
    [Route(HttpVerbs.Post, "/{hotbarId}/{slotId}/execute")]
    public unsafe void TriggerHotbarSlot(int hotbarId, int slotId) {
        var plugin = XIVDeckPlugin.Instance;
        
        var hotbarModule = Framework.Instance()->GetUiModule()->GetRaptureHotbarModule();

        if (!Injections.ClientState.IsLoggedIn)
            throw new InvalidOperationException("A player is not logged in to the game!");

        try {
            this.SafetyCheckHotbar(hotbarId, slotId);
        } catch (ArgumentException ex) {
            throw HttpException.NotAcceptable(ex.Message);
        }

        // Trigger the hotbar event on the next Framework tick, and also in the Framework (game main) thread.
        // For whatever reason, the game *really* doesn't like when a user casts a Weaponskill or Ability from a
        // non-game thread (as would be the case for API calls). Why this works normally for Spells and other
        // actions will forever be a mystery. 
        TickScheduler.Schedule(delegate { 
            var hotbarItem = hotbarModule->HotBar[hotbarId]->Slot[slotId];
            plugin.SigHelper.ExecuteHotbarSlot(hotbarItem);
        });
    }

    private bool SafetyCheckHotbar(int hotbarId, int slotId) {
        switch (hotbarId) {
            // Safety checks
            case < 0 or > 17:
                throw new ArgumentException("Hotbar ID must be between 0 and 17");
            case < 10 when slotId is < 0 or > 11: // normal hotbars
                throw new ArgumentException("When hotbarID < 10, Slot ID must be between 0 and 11");
            case >= 10 when slotId is < 0 or > 15: // cross hotbars
                throw new ArgumentException("When Hotbar ID >= 10, Slot ID must be between 0 and 15");
        }

        return true;
    }
}

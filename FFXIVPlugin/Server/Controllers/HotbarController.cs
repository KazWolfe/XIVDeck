using System;
using EmbedIO;
using EmbedIO.Routing;
using EmbedIO.WebApi;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using XIVDeck.FFXIVPlugin.Base;
using XIVDeck.FFXIVPlugin.Exceptions;
using XIVDeck.FFXIVPlugin.Game;
using XIVDeck.FFXIVPlugin.Resources.Localization;
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
        var iconId = plugin.SigHelper.CalcIconForSlot(hotbarItem);

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
        try {
            this.SafetyCheckHotbar(hotbarId, slotId);
        } catch (ArgumentException ex) { 
            throw HttpException.BadRequest(UIStrings.HotbarController_InvalidHotbarOrSlotError, ex);
        }
        
        // this really should not be here as it's mixing the controller with business logic, but it can't really be
        // put anywhere else either.
        if (!Injections.ClientState.IsLoggedIn)
            throw new PlayerNotLoggedInException();
        
        GameUtils.ResetAFKTimer();
        
        // Trigger the hotbar event on the next Framework tick, and also in the Framework (game main) thread.
        // For whatever reason, the game *really* doesn't like when a user casts a Weaponskill or Ability from a
        // non-game thread (as would be the case for API calls). Why this works normally for Spells and other
        // actions will forever be a mystery. 
        Injections.Framework.RunOnFrameworkThread(delegate { 
            GameUtils.PulseHotbarSlot(hotbarId, slotId);
            Framework.Instance()->UIModule->GetRaptureHotbarModule()->ExecuteSlotById((uint) hotbarId, (uint) slotId);
        });
    }

    private void SafetyCheckHotbar(int hotbarId, int slotId) {
        // ToDo: Set this to be 19 (or do something) once ClientStructs supports pet/extra hotbars.
        if (hotbarId is < 0 or > 17)
            throw new ArgumentException(UIStrings.HotbarController_InvalidHotbarIdError);

        switch (GameUtils.IsCrossHotbar(hotbarId)) {
            case false when slotId is < 0 or > 11:
                throw new ArgumentException(UIStrings.HotbarController_NormalHotbarInvalidSlotError);
            case true when slotId is < 0 or > 15:
                throw new ArgumentException(UIStrings.HotbarController_CrossHotbarInvalidSlotError);
        }
    }
}

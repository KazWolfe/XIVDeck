using System;
using Dalamud.Plugin.Services;
using Dalamud.Utility;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using Serilog;
using XIVDeck.FFXIVPlugin.IoC;
using static FFXIVClientStructs.FFXIV.Client.UI.Misc.RaptureHotbarModule;

namespace XIVDeck.FFXIVPlugin.Game.Managers;

[Service(ServiceFlags.Singleton)]
public class HotbarManager(ILogger log, IGameGui gameGui) {
    public static bool IsCrossHotbar(int hotbarId) {
        return hotbarId switch {
            < 0 or > 19 => throw new ArgumentOutOfRangeException(nameof(hotbarId), @"Hotbar ID must be between 0 and 19."),
            18 => false, // Standard pet/extra hotbar
            19 => true,  // Cross pet/extra hotbar
            _ => hotbarId >= 10
        };
    }

    public static unsafe void ExecuteHotbarAction(HotbarSlotType commandType, uint commandId) {
        ThreadSafety.AssertMainThread();

        var hotbarModulePtr = Framework.Instance()->GetUIModule()->GetRaptureHotbarModule();

        var slot = new HotbarSlot {
            CommandType = commandType,
            CommandId = commandId
        };

        // Note: this is *probably* lifespan-safe. ExecuteSlot shouldn't write anything downstream, but keep in mind.
        hotbarModulePtr->ExecuteSlot(&slot);
    }

    public unsafe void PulseHotbarSlot(int hotbarId, int slotId) {
        var isCrossHotbar = IsCrossHotbar(hotbarId);

        // Handle the main hotbar, which is a bit interesting as it can behave oddly at times.
        var mainBarName = isCrossHotbar ? "_ActionCross" : "_ActionBar";
        var mainBar = (AddonActionBarBase*) gameGui.GetAddonByName(mainBarName).Address;

        if (mainBar != null) {
            if (mainBar->RaptureHotbarId == hotbarId) {
                SafePulseBar(mainBar, slotId);
            }
        } else {
            log.Debug("Couldn't find main hotbar addon {MainBarName}!", mainBarName);
        }

        // And handle any extra visible normal hotbars
        if (!isCrossHotbar && hotbarId != 0) {
            var actionBarName = $"_ActionBar{hotbarId:00}";
            var actionBar = (AddonActionBarBase*) gameGui.GetAddonByName(actionBarName).Address;

            if (actionBar != null) {
                SafePulseBar(actionBar, slotId);
            } else {
                log.Debug("Couldn't find hotbar addon {ActionBarName}!", actionBarName);
            }
        }
    }

    private static unsafe void SafePulseBar(AddonActionBarBase* actionBar, int slotId) {
        if (slotId is < 0 or > 15) {
            return;
        }

        if (!actionBar->AtkUnitBase.IsVisible) {
            return;
        }

        actionBar->PulseActionBarSlot(slotId);
    }

    public static unsafe void ResolveApparentAction(HotbarSlot* slot, out HotbarSlotType actionType, out uint actionId) {
        // short circuit, just a micro-optimization.
        if (slot->CommandType == 0 && slot->CommandId == 0) {
            actionType = HotbarSlotType.Empty;
            actionId = 0;

            return;
        }

        var hotbarModule = Framework.Instance()->GetUIModule()->GetRaptureHotbarModule();

        // Take in default values, just in case GetSlotAppearance fails for some reason
        var acType = slot->ApparentSlotType;
        var acId = slot->ApparentActionId;
        ushort actionModeParam = slot->ApparentActionModeParam;

        RaptureHotbarModule.GetSlotAppearance(&acType, &acId, &actionModeParam, hotbarModule, slot);

        actionType = acType;
        actionId = acId;
    }

    /// <summary>
    /// Fixed variant of game's GetSlotById, accounting for the pet cross hotbar.
    /// </summary>
    /// <param name="hotbarId">Hotbar ID, 0 to 19, inclusive.</param>
    /// <param name="slotId">Slot ID.</param>
    /// <returns>The referenced slot.</returns>
    public static unsafe RaptureHotbarModule.HotbarSlot* GetSlotByIdFixed(uint hotbarId, uint slotId) {
        var module = RaptureHotbarModule.Instance();
        return hotbarId switch {
            < 18 => module->GetSlotById(hotbarId, slotId),
            18 => module->PetHotbar.GetHotbarSlot(slotId),
            19 => module->PetCrossHotbar.GetHotbarSlot(slotId),
            _ => throw new ArgumentOutOfRangeException(nameof(hotbarId), @"Hotbar ID is not valid")
        };
    }
}

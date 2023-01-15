using System;
using System.Runtime.InteropServices;
using Dalamud.Logging;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using XIVDeck.FFXIVPlugin.Base;

namespace XIVDeck.FFXIVPlugin.Game.Managers; 

internal static class HotbarManager {
    public static bool IsCrossHotbar(int hotbarId) {
        return hotbarId switch {
            < 0 or > 19 => throw new ArgumentOutOfRangeException(nameof(hotbarId), @"Hotbar ID must be between 0 and 19."),
            18 => false, // Standard pet/extra hotbar
            19 => true,  // Cross pet/extra hotbar
            _ => hotbarId >= 10
        };
    }

    public static unsafe void ExecuteHotbarAction(HotbarSlotType commandType, uint commandId) {
        var hotbarModulePtr = Framework.Instance()->GetUiModule()->GetRaptureHotbarModule();

        var slot = new HotBarSlot {
            CommandType = commandType,
            CommandId = commandId
        };

        var ptr = Marshal.AllocHGlobal(Marshal.SizeOf(slot));
        Marshal.StructureToPtr(slot, ptr, false);

        hotbarModulePtr->ExecuteSlot((HotBarSlot*) ptr);

        Marshal.FreeHGlobal(ptr);
    }

    public static unsafe void PulseHotbarSlot(int hotbarId, int slotId) {
        var isCrossHotbar = IsCrossHotbar(hotbarId);
        
        // Handle the main hotbar, which is a bit interesting as it can behave oddly at times.
        var mainBarName = isCrossHotbar ? "_ActionCross" : "_ActionBar";
        var mainBarPtr = Injections.GameGui.GetAddonByName(mainBarName);

        if (mainBarPtr != nint.Zero) {
            var activeHotbarId = *(byte*) (mainBarPtr + 0x23C); // offset to RaptureHotbarId

            if (activeHotbarId == hotbarId) {
                SafePulseBar((AddonActionBarBase*) mainBarPtr, slotId);
            }
        } else {
            PluginLog.Debug($"Couldn't find main hotbar addon {mainBarName}!");
        }

        // And handle any extra visible normal hotbars
        if (!isCrossHotbar) {
            var actionBarName = $"_ActionBar{hotbarId:00}";
            var actionBarPtr = Injections.GameGui.GetAddonByName(actionBarName);

            if (actionBarPtr != nint.Zero) {
                SafePulseBar((AddonActionBarBase*) actionBarPtr, slotId);
            } else {
                PluginLog.Debug($"Couldn't find hotbar addon {actionBarName}");
            }
        }
    }
    
    public static unsafe int CalcIconForSlot(HotBarSlot* slot) {
        if (slot->CommandType == HotbarSlotType.Empty) {
            return 0;
        }

        CalcBForSlot(slot, out var slotActionType, out var slotActionId);

        return slot->GetIconIdForSlot(slotActionType, slotActionId);
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

    public static unsafe void CalcBForSlot(HotBarSlot* slot, out HotbarSlotType actionType, out uint actionId) {
        var hotbarModule = Framework.Instance()->GetUiModule()->GetRaptureHotbarModule();

        // Take in default values, just in case GetSlotAppearance fails for some reason
        var acType = slot->IconTypeB;
        var acId = slot->IconB;
        ushort actionCost = slot->UNK_0xCA;
        
        RaptureHotbarModule.GetSlotAppearance(&acType, &acId, &actionCost, hotbarModule, slot);

        actionType = acType;
        actionId = acId;
    }
}
using System;
using System.Runtime.InteropServices;
using Dalamud.Logging;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using XIVDeck.FFXIVPlugin.Base;
using XIVDeck.FFXIVPlugin.Utils;

namespace XIVDeck.FFXIVPlugin.Game; 

public static class GameUtils {
    public static bool IsCrossHotbar(int hotbarId) {
        return hotbarId switch {
            < 0 or > 19 => throw new ArgumentOutOfRangeException(nameof(hotbarId), @"Hotbar ID must be between 0 and 19."),
            18 => false, // Standard pet/extra hotbar
            19 => true,  // Cross pet/extra hotbar
            _ => hotbarId >= 10
        };
    }
    
    // borrowed base logic from ChatTwo by ascclemens
    public static void SendSanitizedChatMessage(string text, bool commandOnly = true) {
        var plugin = XIVDeckPlugin.Instance;
            
        if (commandOnly && !text.StartsWith("/")) {
            throw new ArgumentException("The specified message message does not start with a slash while in command-only mode.");
        }
            
        // sanitization rules
        text = text.Replace("\n", " ");
        text = plugin.SigHelper.GetSanitizedString(text);
            
        plugin.SigHelper.SendChatMessage(text);
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

    public static void ResetAFKTimer() {
        if (!InputUtil.TryFindGameWindow(out var windowHandle)) return;
        
        // Virtual key for Right Winkey. Can't be used by FFXIV normally, and in tests did not seem to cause any
        // unusual interference.
        InputUtil.SendKeycode(windowHandle, 0x5C);
    }
    
    public static unsafe void PulseHotbarSlot(int hotbarId, int slotId) {
        var isCrossHotbar = IsCrossHotbar(hotbarId);
        
        // Handle the main hotbar, which is a bit interesting as it can behave oddly at times.
        var mainBarName = isCrossHotbar ? "_ActionCross" : "_ActionBar";
        var mainBarPtr = Injections.GameGui.GetAddonByName(mainBarName, 1);

        if (mainBarPtr != IntPtr.Zero) {
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
            var actionBarPtr = Injections.GameGui.GetAddonByName(actionBarName, 1);

            if (actionBarPtr != IntPtr.Zero) {
               SafePulseBar((AddonActionBarBase*) actionBarPtr, slotId);
            } else {
                PluginLog.Debug($"Couldn't find hotbar addon {actionBarName}");
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

}
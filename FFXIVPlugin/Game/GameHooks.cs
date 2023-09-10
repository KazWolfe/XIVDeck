using System;
using Dalamud.Hooking;

using Dalamud.Utility.Signatures;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using XIVDeck.FFXIVPlugin.Base;
using XIVDeck.FFXIVPlugin.Server.Messages.Outbound;

// ReSharper disable InconsistentNaming - matching expected documentation things
// ReSharper disable UnusedAutoPropertyAccessor.Local - handled by siggingway and reflection

namespace XIVDeck.FFXIVPlugin.Game;

internal unsafe class GameHooks : IDisposable {
    private static class Signatures {
        // todo: this is *way* too broad. this game is very write-happy when it comes to gearset updates.
        internal const string SaveGearset = "48 89 5C 24 ?? 48 89 74 24 ?? 57 48 83 EC 30 48 8B F2 48 8B F9 33 D2";
    }

    /***** hooks *****/
    private delegate nint RaptureGearsetModule_WriteFile(nint a1, nint a2);

    private delegate nint MacroUpdate(RaptureMacroModule* self, byte needsSave, uint set);

    [Signature(Signatures.SaveGearset, DetourName = nameof(DetourGearsetSave))]
    private Hook<RaptureGearsetModule_WriteFile>? RGM_WriteFileHook { get; init; }

    private readonly Hook<MacroUpdate>? MacroUpdateHook;

    /***** the actual class *****/

    internal GameHooks() {
        SignatureHelper.Initialise(this);

        this.RGM_WriteFileHook?.Enable();

        var macroUpdateFPtr = RaptureMacroModule.Addresses.SetSavePendingFlag.Value;
        this.MacroUpdateHook = Hook<MacroUpdate>.FromAddress((nint) macroUpdateFPtr, this.DetourMacroUpdate);
        this.MacroUpdateHook.Enable();
    }

    public void Dispose() {
        this.RGM_WriteFileHook?.Dispose();
        this.MacroUpdateHook?.Dispose();

        GC.SuppressFinalize(this);
    }

    private nint DetourGearsetSave(nint a1, nint a2) {
        Injections.PluginLog.Debug("Detected a gearset update; broadcasting event.");

        try {
            XIVDeckPlugin.Instance.Server.BroadcastMessage(new WSStateUpdateMessage("GearSet"));
        } catch (Exception ex) {
            Injections.PluginLog.Error(ex, "Gearset update notification on hook failed");
        }

        return this.RGM_WriteFileHook!.Original(a1, a2);
    }

    private nint DetourMacroUpdate(RaptureMacroModule* self, byte needsSave, uint set) {
        Injections.PluginLog.Debug("Detected a macro update; broadcasting event.");
        
        try {
            XIVDeckPlugin.Instance.Server.BroadcastMessage(new WSStateUpdateMessage("Macro"));
        } catch (Exception ex) {
            Injections.PluginLog.Error(ex, "Macro update notification on hook failed");
        }

        return this.MacroUpdateHook!.Original(self, needsSave, set);
    }
}
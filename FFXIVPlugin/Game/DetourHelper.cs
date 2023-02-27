using System;
using Dalamud.Hooking;
using Dalamud.Logging;
using Dalamud.Utility.Signatures;
using XIVDeck.FFXIVPlugin.Server;
using XIVDeck.FFXIVPlugin.Server.Messages.Outbound;

// ReSharper disable InconsistentNaming - matching expected documentation things
// ReSharper disable UnusedAutoPropertyAccessor.Local - handled by siggingway and reflection

namespace XIVDeck.FFXIVPlugin.Game;

internal class DetourHelper : IDisposable {
    private static class Signatures {
        // todo: this is *way* too broad. this game is very write-happy when it comes to gearset updates.
        internal const string SaveGearset = "48 89 5C 24 ?? 48 89 74 24 ?? 57 48 83 EC 30 48 8B F2 48 8B F9 33 D2";

        // This signature appears to be called whenever the Macro Agent does some form of update operation on a macro.
        // I'm not exactly sure what this method docs, but it seems to be relatively reliable.
        // According to aers, this might be related to marking the macro page as modified for file update purposes.
        // called in UI::Agent::AgentMacro::ReceiveEvent a few times - generally immediately after LOBYTE(x) = 1 call
        internal const string UpdateMacro = "45 85 C0 75 04 88 51 3D";
    }

    /***** hooks *****/
    private delegate nint RaptureGearsetModule_WriteFile(nint a1, nint a2);
    private delegate nint MacroUpdate(nint a1, nint macroPage, nint macroNumber);

    [Signature(Signatures.SaveGearset, DetourName = nameof(DetourGearsetSave))]
    private Hook<RaptureGearsetModule_WriteFile>? RGM_WriteFileHook { get; init; }

    [Signature(Signatures.UpdateMacro, DetourName = nameof(DetourMacroUpdate))]
    private Hook<MacroUpdate>? MacroUpdateHook { get; init; }

    /***** the actual class *****/

    internal DetourHelper() {
        SignatureHelper.Initialise(this);

        this.RGM_WriteFileHook?.Enable();
        this.MacroUpdateHook?.Enable();
    }

    public void Dispose() {
        this.RGM_WriteFileHook?.Dispose();
        this.MacroUpdateHook?.Dispose();

        GC.SuppressFinalize(this);
    }

    private nint DetourGearsetSave(nint a1, nint a2) {
        PluginLog.Debug("Gearset update!");
        var tmp = this.RGM_WriteFileHook!.Original(a1, a2);

        try {
            XIVDeckWSServer.Instance?.BroadcastMessage(new WSStateUpdateMessage("GearSet"));
        } catch (Exception ex) {
            PluginLog.Error(ex, "Gearset update notification on hook failed");
        }

        return tmp;
    }

    private nint DetourMacroUpdate(nint a1, nint macroPage, nint macroSlot) {
        PluginLog.Debug("Macro update!");
        var tmp = this.MacroUpdateHook!.Original(a1, macroPage, macroSlot);

        try {
            XIVDeckWSServer.Instance?.BroadcastMessage(new WSStateUpdateMessage("Macro"));
        } catch (Exception ex) {
            PluginLog.Error(ex, "Macro update notification on hook failed");
        }

        return tmp;
    }
}
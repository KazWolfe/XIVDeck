using System;
using Dalamud.Hooking;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using XIVDeck.FFXIVPlugin.Base;
using XIVDeck.FFXIVPlugin.Server.Messages.Outbound;

// ReSharper disable InconsistentNaming - matching expected documentation things
// ReSharper disable UnusedAutoPropertyAccessor.Local - handled by siggingway and reflection

namespace XIVDeck.FFXIVPlugin.Game;

internal unsafe class GameHooks : IDisposable {
    private readonly Hook<RaptureGearsetModule.Delegates.WriteFile>? GearsetUpdateHook;
    private readonly Hook<RaptureMacroModule.Delegates.SetSavePendingFlag>? MacroUpdateHook;

    internal GameHooks() {
        Injections.GameInteropProvider.InitializeFromAttributes(this);

        this.GearsetUpdateHook =
            Injections.GameInteropProvider.HookFromAddress<RaptureGearsetModule.Delegates.WriteFile>(
                (nint)RaptureGearsetModule.StaticVirtualTablePointer->WriteFile,
                this.DetourGearsetSave);
        this.GearsetUpdateHook?.Enable();

        this.MacroUpdateHook = Injections.GameInteropProvider
            .HookFromAddress<RaptureMacroModule.Delegates.SetSavePendingFlag>(
                (nint)RaptureMacroModule.MemberFunctionPointers.SetSavePendingFlag,
                this.DetourMacroUpdate);
        this.MacroUpdateHook.Enable();
    }

    public void Dispose() {
        this.GearsetUpdateHook?.Dispose();
        this.MacroUpdateHook?.Dispose();

        GC.SuppressFinalize(this);
    }

    private uint DetourGearsetSave(RaptureGearsetModule* self, byte* ptr, uint length) {
        Injections.PluginLog.Debug("Detected a gearset update; broadcasting event.");

        try {
            XIVDeckPlugin.Instance.Server.BroadcastMessage(new WSStateUpdateMessage("GearSet"));
        } catch (Exception ex) {
            Injections.PluginLog.Error(ex, "Gearset update notification on hook failed");
        }

        return this.GearsetUpdateHook!.Original(self, ptr, length);
    }

    private void DetourMacroUpdate(RaptureMacroModule* self, bool needsSave, uint set) {
        Injections.PluginLog.Debug("Detected a macro update; broadcasting event.");

        try {
            XIVDeckPlugin.Instance.Server.BroadcastMessage(new WSStateUpdateMessage("Macro"));
        } catch (Exception ex) {
            Injections.PluginLog.Error(ex, "Macro update notification on hook failed");
        }

        this.MacroUpdateHook!.Original(self, needsSave, set);
    }
}

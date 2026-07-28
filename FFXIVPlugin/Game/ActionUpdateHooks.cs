using System;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Hooking;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using Serilog;
using XIVDeck.FFXIVPlugin.Game.Types;
using XIVDeck.FFXIVPlugin.IoC;
using XIVDeck.FFXIVPlugin.Utils;

namespace XIVDeck.FFXIVPlugin.Game;

[Service(ServiceFlags.Singleton)]
public unsafe class ActionUpdateHooks : IDisposable {
    private static readonly uint[] DutyActionIds = [26u, 27u];
    private static readonly uint[] PhantomActionIds = [31u, 32u, 33u, 34u, 35u];

    private readonly ILogger _log;

    public event EventHandler<IEnumerable<ActionUpdateEvent>>? ActionUpdate;

    private readonly Hook<RaptureGearsetModule.Delegates.WriteFile>? _gearsetUpdateHook;
    private readonly Hook<RaptureMacroModule.Delegates.SetSavePendingFlag>? _macroUpdateHook;

    private readonly Hook<RaptureHotbarModule.Delegates.SetDutyActionsPresent>? _dutyActionPresenceHook;
    private readonly Hook<RaptureHotbarModule.Delegates.SetPhantomActionsPresent>? _phantomActionPresenceHook;

    public ActionUpdateHooks(ILogger logger, IGameInteropProvider interopProvider) {
        this._log = logger;

        this._gearsetUpdateHook =
            interopProvider.HookFromAddress<RaptureGearsetModule.Delegates.WriteFile>(
                (nint)RaptureGearsetModule.StaticVirtualTablePointer->WriteFile,
                this.DetourGearsetSave);
        this._gearsetUpdateHook?.Enable();

        this._macroUpdateHook = interopProvider
            .HookFromAddress<RaptureMacroModule.Delegates.SetSavePendingFlag>(
                RaptureMacroModule.Addresses.SetSavePendingFlag.Value,
                this.DetourMacroUpdate);
        this._macroUpdateHook?.Enable();

        this._dutyActionPresenceHook = interopProvider
            .HookFromAddress<RaptureHotbarModule.Delegates.SetDutyActionsPresent>(
                RaptureHotbarModule.Addresses.SetDutyActionsPresent.Value,
                this.DetourDutyActionPresence
            );
        this._dutyActionPresenceHook?.Enable();

        this._phantomActionPresenceHook = interopProvider
            .HookFromAddress<RaptureHotbarModule.Delegates.SetPhantomActionsPresent>(
                RaptureHotbarModule.Addresses.SetPhantomActionsPresent.Value,
                this.DetourPhantomActionPresence);
        this._phantomActionPresenceHook?.Enable();
    }

    public void Dispose() {
        this._gearsetUpdateHook?.Dispose();
        this._macroUpdateHook?.Dispose();
        this._dutyActionPresenceHook?.Dispose();
        this._phantomActionPresenceHook?.Dispose();

        GC.SuppressFinalize(this);
    }

    private uint DetourGearsetSave(RaptureGearsetModule* self, byte* ptr, uint length) {
        this._log.Debug("Detected a gearset update; broadcasting event.");

        this.ActionUpdate?.InvokeSafely(this, new[] {
            new ActionUpdateEvent { SlotType = RaptureHotbarModule.HotbarSlotType.GearSet }
        }, ex => { this._log.Error(ex, "Gearset update notification on hook failed"); });

        return this._gearsetUpdateHook!.Original(self, ptr, length);
    }

    private void DetourMacroUpdate(RaptureMacroModule* self, bool needsSave, uint set) {
        this._log.Debug("Detected a macro update; broadcasting event.");

        this.ActionUpdate?.InvokeSafely(this, [
            new ActionUpdateEvent { SlotType = RaptureHotbarModule.HotbarSlotType.Macro }
        ], ex => { this._log.Error(ex, "Macro update notification on hook failed"); });

        this._macroUpdateHook!.OriginalDisposeSafe(self, needsSave, set);
    }

    private void DetourDutyActionPresence(RaptureHotbarModule* self, bool presence) {
        this.ActionUpdate.InvokeSafely(this, DutyActionIds.Select(actionId => new ActionUpdateEvent {
            SlotType = RaptureHotbarModule.HotbarSlotType.GeneralAction,
            ActionId = actionId,
        }), ex => { this._log.Error(ex, "Error invoking duty action update."); });

        this._dutyActionPresenceHook!.OriginalDisposeSafe(self, presence);
    }

    private void DetourPhantomActionPresence(RaptureHotbarModule* self, bool presence) {
        this.ActionUpdate.InvokeSafely(this, PhantomActionIds.Select(actionId => new ActionUpdateEvent {
            SlotType = RaptureHotbarModule.HotbarSlotType.GeneralAction,
            ActionId = actionId,
        }), ex => { this._log.Error(ex, "Error invoking phantom action update."); });

        this._phantomActionPresenceHook!.OriginalDisposeSafe(self, presence);
    }
}

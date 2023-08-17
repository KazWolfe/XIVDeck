using System;
using Dalamud.Hooking;
using Dalamud.Logging;
using Dalamud.Utility.Signatures;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using XIVDeck.FFXIVPlugin.Server.Messages.Outbound;
using XIVDeck.FFXIVPlugin.Server.Types;

// ReSharper disable InconsistentNaming - matching expected documentation things
// ReSharper disable UnusedAutoPropertyAccessor.Local - handled by siggingway and reflection

namespace XIVDeck.FFXIVPlugin.Game;

internal unsafe class GameHooks : IDisposable {
    private static class Signatures {
        // todo: this is *way* too broad. this game is very write-happy when it comes to gearset updates.
        internal const string SaveGearset = "48 89 5C 24 ?? 48 89 74 24 ?? 57 48 83 EC 30 48 8B F2 48 8B F9 33 D2";

        internal const string StartCooldown = "E8 ?? ?? ?? ?? 48 8B 05 ?? ?? ?? ?? 48 8D 0D ?? ?? ?? ?? FF 50 18";
        internal const string SyncCooldown = "E8 ?? ?? ?? ?? 8B 4B 28 8D 41 FF";
    }

    /***** hooks *****/
    private delegate nint RaptureGearsetModule_WriteFile(nint a1, nint a2);

    private delegate nint MacroUpdate(RaptureMacroModule* self, byte needsSave, uint set);
    
    private delegate nint StartCooldown(ActionManager* self, ActionType actionType, uint actionId);
    private delegate nint SyncCooldown(ActionManager* self, ActionType actionType, uint actionId);

    [Signature(Signatures.SaveGearset, DetourName = nameof(DetourGearsetSave))]
    private Hook<RaptureGearsetModule_WriteFile>? RGM_WriteFileHook { get; init; }
    
    [Signature(Signatures.StartCooldown, DetourName = nameof(StartCooldownDetour))]
    private Hook<StartCooldown>? StartCooldownHook { get; init; }
    
    [Signature(Signatures.SyncCooldown, DetourName = nameof(SyncCooldownDetour))]
    private Hook<SyncCooldown>? SyncCooldownHook { get; init; }

    private readonly Hook<MacroUpdate>? MacroUpdateHook;

    /***** the actual class *****/

    internal GameHooks() {
        SignatureHelper.Initialise(this);

        this.RGM_WriteFileHook?.Enable();

        var macroUpdateFPtr = RaptureMacroModule.Addresses.SetSavePendingFlag.Value;
        this.MacroUpdateHook = Hook<MacroUpdate>.FromAddress((nint) macroUpdateFPtr, this.DetourMacroUpdate);
        this.MacroUpdateHook.Enable();
        
        this.StartCooldownHook?.Enable();
        this.SyncCooldownHook?.Enable();
    }

    public void Dispose() {
        this.RGM_WriteFileHook?.Dispose();
        this.MacroUpdateHook?.Dispose();
        this.StartCooldownHook?.Dispose();
        this.SyncCooldownHook?.Dispose();

        GC.SuppressFinalize(this);
    }

    private nint DetourGearsetSave(nint a1, nint a2) {
        PluginLog.Debug("Detected a gearset update; broadcasting event.");

        try {
            XIVDeckPlugin.Instance.Server.BroadcastMessage(new WSStateUpdateMessage("GearSet"));
        } catch (Exception ex) {
            PluginLog.Error(ex, "Gearset update notification on hook failed");
        }

        return this.RGM_WriteFileHook!.Original(a1, a2);
    }

    private nint DetourMacroUpdate(RaptureMacroModule* self, byte needsSave, uint set) {
        PluginLog.Debug("Detected a macro update; broadcasting event.");
        
        try {
            XIVDeckPlugin.Instance.Server.BroadcastMessage(new WSStateUpdateMessage("Macro"));
        } catch (Exception ex) {
            PluginLog.Error(ex, "Macro update notification on hook failed");
        }

        return this.MacroUpdateHook!.Original(self, needsSave, set);
    }

    private nint StartCooldownDetour(ActionManager* actionManager, ActionType actionType, uint actionId) {
        var result = this.StartCooldownHook!.Original(actionManager, actionType, actionId);

        try {
            this.BroadcastCooldownUpdate(actionType, actionId);
        } catch (Exception ex) {
            PluginLog.Error(ex, "Start cooldown hook failed");
        }
        
        return result;
    }

    private nint SyncCooldownDetour(ActionManager* actionManager, ActionType actionType, uint actionId) {
        var result = this.SyncCooldownHook!.Original(actionManager, actionType, actionId);

        try {
            this.BroadcastCooldownUpdate(actionType, actionId);
        } catch (Exception ex) {
            PluginLog.Error(ex, "Sync cooldown hook failed");
        }
        
        return result;
    }

    private void BroadcastCooldownUpdate(ActionType actionType, uint actionId) {
        var actionManager = ActionManager.Instance();
        
        var recastGroupId = actionManager->GetRecastGroup((int) actionType, actionId);
        var recastGroup = actionManager->GetRecastGroupDetail(recastGroupId);
        if (recastGroup == null) return;

        var cooldownInfo = new CooldownInfo {
            CooldownGroupId = recastGroupId,
            CooldownActive = recastGroup->IsActive > 0,
            LastActionId = recastGroup->ActionID,
            ElapsedChargeTime = recastGroup->Elapsed,
            AdjustedRecastTime = ActionManager.GetAdjustedRecastTime(actionType, actionId) / 1000f,
        };

        if (recastGroup->ActionID != 0) {
            cooldownInfo.MaxCharges = ActionManager.GetMaxCharges(recastGroup->ActionID, 0);
        }

        XIVDeckPlugin.Instance.Server.BroadcastMessage(new WSCooldownGroupUpdateMessage(cooldownInfo));
    }
}
using System;
using System.Numerics;
using Dalamud.Hooking;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using Serilog;
using XIVDeck.FFXIVPlugin.IoC;
using XIVDeck.FFXIVPlugin.Utils;
using CooldownNotification = XIVDeck.FFXIVPlugin.Contract.CooldownNotification;
using XIVDeck.FFXIVPlugin.Contract;

namespace XIVDeck.FFXIVPlugin.Game.Managers;

[Service(ServiceFlags.Singleton | ServiceFlags.AutoLoad)]
public unsafe class CooldownManager : IDisposable {
    private readonly ILogger _log;

    public event EventHandler<CooldownNotification>? CooldownStart;

    private Hook<ActionManager.Delegates.UseActionLocation>? _useActionHook;
    private Hook<ActionManager.Delegates.SetCooldown>? _setCooldownHook;
    private Hook<ActionManager.Delegates.OnCastCancelled>? _castCancelledHook;

    private Hook<AgentInventoryContext.Delegates.UseItem>? _useItemHook;

    public CooldownManager(ILogger logger, IGameInteropProvider interop) {
        this._log = logger;

        this._useActionHook = interop.HookFromAddress<ActionManager.Delegates.UseActionLocation>(
            ActionManager.Addresses.UseActionLocation.Value, this.DetourUseActionLocation);
        this._useActionHook?.Enable();

        this._setCooldownHook = interop.HookFromAddress<ActionManager.Delegates.SetCooldown>(
            ActionManager.Addresses.SetCooldown.Value, this.DetourServerSetCooldown);
        this._setCooldownHook?.Enable();

        this._castCancelledHook = interop.HookFromAddress<ActionManager.Delegates.OnCastCancelled>(
            ActionManager.Addresses.OnCastCancelled.Value, this.DetourCastCancelled);
        this._castCancelledHook?.Enable();

        this._useItemHook = interop.HookFromAddress<AgentInventoryContext.Delegates.UseItem>(
            AgentInventoryContext.Addresses.UseItem.Value, this.DetourUseItem);
        this._useItemHook?.Enable();
    }

    public void Dispose() {
        this._useItemHook?.Dispose();
        this._useItemHook = null;

        this._castCancelledHook?.Dispose();
        this._castCancelledHook = null;

        this._setCooldownHook?.Dispose();
        this._setCooldownHook = null;

        this._useActionHook?.Dispose();
        this._useActionHook = null;

        GC.SuppressFinalize(this);
    }


    /// <param name="slot">The slot, used to translate its slot type into a game action type.</param>
    /// <param name="apparentType">The slot's apparent action type, as resolved by the caller.</param>
    /// <param name="apparentId">The slot's apparent action ID, as resolved by the caller.</param>
    public ActionCooldownDetail GetActionCooldownSnapshotForSlot(RaptureHotbarModule.HotbarSlot* slot,
        RaptureHotbarModule.HotbarSlotType apparentType, uint apparentId) {
        return GetActionCooldownSnapshot(slot->GetActionTypeForSlotType(apparentType), apparentId);
    }

    private bool DetourUseActionLocation(ActionManager* thisPtr, ActionType actionType, uint actionId, ulong targetId,
        Vector3* location, uint extraParam, byte a7) {
        var result = this._useActionHook!.OriginalDisposeSafe(thisPtr, actionType, actionId, targetId, location,
            extraParam, a7);

        if (!result) return result;

        this.OnActionFired(thisPtr, actionType, actionId);

        return result;
    }

    private void DetourServerSetCooldown(ActionManager* self, ActionType type, uint actionId, float elapsed, float total) {
        this._setCooldownHook!.OriginalDisposeSafe(self, type, actionId, elapsed, total);
        this.OnActionFired(self, type, actionId);
    }

    private void DetourCastCancelled(ActionManager* thisPtr) {
        // castcancelled (obviously) clears these, so save them now.
        var savedCastType = thisPtr->CastActionType;
        var savedCastId = thisPtr->CastActionId;

        this._castCancelledHook!.OriginalDisposeSafe(thisPtr);
        this.OnActionFired(thisPtr, savedCastType, savedCastId);
    }

    private long DetourUseItem(AgentInventoryContext* thisPtr, uint itemId, InventoryType inventoryType, uint itemSlot = 0,
        short a5 = 0) {
        var result = this._useItemHook!.OriginalDisposeSafe(thisPtr, itemId, inventoryType, itemSlot, a5);

        this.OnActionFired(ActionManager.Instance(), ActionType.Item, itemId);
        return result;
    }

    private void OnActionFired(ActionManager* actionManager, ActionType type, uint actionId) {
        try {
            this.EmitCooldownStart(actionManager->GetRecastGroup((int)type, actionId));
            this.EmitCooldownStart(actionManager->GetAdditionalRecastGroup(type, actionId));
        } catch (Exception ex) {
            this._log.Error(ex, "Caught an exception trying to handle a cooldown start event.");
        }
    }

    private void EmitCooldownStart(int groupId) {
        if (groupId < 0) return;

        this.CooldownStart?.InvokeSafely(this, new CooldownNotification(groupId));
    }

    private static ActionCooldownDetail GetActionCooldownSnapshot(ActionType actionType, uint actionId) {
        var actionManager = ActionManager.Instance();
        var now = DateTimeOffset.UtcNow;

        var hci = new HotbarCooldownInfo();
        actionManager->GetHotbarCooldownInfo(&hci, actionType, actionId);

        var snapshot = new ActionCooldownDetail {
            ActionType = actionType,
            ActionId = actionId,
        };

        var primaryGroupId = actionManager->GetRecastGroup((int)actionType, actionId);
        var additionalGroupId = actionManager->GetAdditionalRecastGroup(actionType, actionId);

        var renderMode = hci.DisplayType switch {
            CooldownDisplayType.GcdMultiCharge or CooldownDisplayType.GcdSingleCharge => CooldownRenderMode.ChargeRing,
            CooldownDisplayType.SpecialCharge => CooldownRenderMode.ChargeSweep,
            _ => CooldownRenderMode.RecastSweep,
        };

        var currentCharges = actionType == ActionType.Action
            ? (int)actionManager->GetCurrentCharges(actionId)
            : 0;

        var (primaryStart, primaryEnd) = CooldownWindow(hci.PrimaryElapsedTime, hci.PrimaryTotalTime, now);
        var primaryDetail = new CooldownGroupDetail {
            GroupId = primaryGroupId,
            RenderMode = renderMode,
            StartTime = primaryStart,
            EndTime = primaryEnd,
            CurrentCharges = currentCharges,
            MaxCharges = hci.CurrentMaxCharges,
            HideTimerLabel = hci.IsGcd,
        };

        if (additionalGroupId < 0) {
            snapshot.RecastGroup = primaryDetail;
        } else {
            snapshot.RechargeGroup = primaryDetail;

            var additionalActive = actionManager->GetRecastGroupDetail(additionalGroupId)->IsActive;
            var (additionalStart, additionalEnd) = CooldownWindow(hci.AdditionalElapsedTime,
                hci.AdditionalTotalTime, now);

            snapshot.RecastGroup = new CooldownGroupDetail {
                GroupId = additionalGroupId,
                RenderMode = additionalActive ? CooldownRenderMode.RecastSweep : CooldownRenderMode.None,
                StartTime = additionalStart,
                EndTime = additionalEnd,
                HideTimerLabel = additionalGroupId == 57,
            };
        }

        return snapshot;
    }

    private static (long StartTime, long EndTime) CooldownWindow(float elapsed, float total, DateTimeOffset now) {
        var startTime = now.ToUnixTimeMilliseconds() - (long)(elapsed * 1000);
        var endTime = startTime + (long)(total * 1000);

        return (startTime, endTime);
    }
}

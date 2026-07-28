using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using Lumina.Excel.Sheets;
using Serilog;
using XIVDeck.FFXIVPlugin.Exceptions;
using XIVDeck.FFXIVPlugin.Game;
using XIVDeck.FFXIVPlugin.Game.Managers;
using XIVDeck.FFXIVPlugin.Game.Watchers;
using XIVDeck.FFXIVPlugin.Resources.Localization;
using XIVDeck.FFXIVPlugin.RpcServer.Helpers;
using XIVDeck.FFXIVPlugin.Contract;
using ActionAppearance = XIVDeck.FFXIVPlugin.Contract.ActionAppearance;
using Task = System.Threading.Tasks.Task;
using XIVDeck.FFXIVPlugin.Game.Types;

namespace XIVDeck.FFXIVPlugin.RpcServer.Services;

[RpcService("Hotbar")]
public class HotbarService : IDisposable {
    private readonly ILogger _log;
    private readonly IClientState _clientState;
    private readonly IFramework _framework;
    private readonly HotbarWatcher.Registration _watch;
    private readonly HotbarManager _hotbarManager;
    private readonly ActionAppearanceResolver _appearanceResolver;

    public event EventHandler<HotbarChangeNotification>? OnHotbarChanged;

    public HotbarService(ILogger log, IClientState clientState, IFramework framework, HotbarWatcher hotbarWatcher,
        HotbarManager hotbarManager, ActionAppearanceResolver appearanceResolver) {
        this._log = log;
        this._clientState = clientState;
        this._framework = framework;
        this._watch = hotbarWatcher.Register(this.OnWatchedSlotsChanged);
        this._hotbarManager = hotbarManager;
        this._appearanceResolver = appearanceResolver;
    }

    public void Dispose() {
        this._watch.Dispose();
        GC.SuppressFinalize(this);
    }

    public void SetWatchedSlots(HotbarWatchRequest watchRequest) {
        var slots = watchRequest.Slots ?? [];

        foreach (var slot in slots) {
            ValidateHotbarSlot(slot.HotbarId, slot.SlotId);
        }

        this._watch.SetSlots(slots);
    }

    private void OnWatchedSlotsChanged(IReadOnlyList<HotbarSlotRef> changedSlots) {
        this.OnHotbarChanged?.Invoke(this, new HotbarChangeNotification([.. changedSlots]));
    }

    public async Task<ActionAppearance> GetHotbarSlot(int hotbarId, int slotId) {
        ValidateHotbarSlot(hotbarId, slotId);

        return await this._framework.RunOnFrameworkThread(() => this.ReadHotbarSlot(hotbarId, slotId));
    }

    private unsafe ActionAppearance ReadHotbarSlot(int hotbarId, int slotId) {
        return this._appearanceResolver.GetAppearance(HotbarManager.GetSlotByIdFixed((uint)hotbarId, (uint)slotId));
    }

    public async Task TriggerHotbarSlot(int hotbarId, int slotId) {
        ValidateHotbarSlot(hotbarId, slotId);

        if (!this._clientState.IsLoggedIn)
            throw new PlayerNotLoggedInException();

        GameUtils.SendDummyInput();

        // Trigger the hotbar event on the next Framework tick, and also in the Framework (game main) thread.
        // For whatever reason, the game *really* doesn't like when a user casts a Weaponskill or Ability from a
        // non-game thread (as would be the case for API calls). Why this works normally for Spells and other
        // actions will forever be a mystery.
        await this._framework.RunOnFrameworkThread(() => this.ExecuteHotbarSlotUnsafe(hotbarId, slotId));
    }

    private unsafe void ExecuteHotbarSlotUnsafe(int hotbarId, int slotId) {
        this._hotbarManager.PulseHotbarSlot(hotbarId, slotId);
        RaptureHotbarModule.Instance()->ExecuteSlot(HotbarManager.GetSlotByIdFixed((uint)hotbarId, (uint)slotId));
    }

    private static void ValidateHotbarSlot(int hotbarId, int slotId) {
        if (hotbarId is < 0 or > 19)
            throw new ActionInvalidException(UIStrings.HotbarService_InvalidHotbarIdError);

        switch (HotbarManager.IsCrossHotbar(hotbarId)) {
            case false when slotId is < 0 or > 11:
                throw new ActionInvalidException(UIStrings.HotbarService_NormalHotbarInvalidSlotError);
            case true when slotId is < 0 or > 15:
                throw new ActionInvalidException(UIStrings.HotbarService_CrossHotbarInvalidSlotError);
        }
    }
}

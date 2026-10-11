using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dalamud.Game.Gui;
using Dalamud.Plugin.Services;
using Serilog;
using static FFXIVClientStructs.FFXIV.Client.UI.Misc.RaptureHotbarModule;
using XIVDeck.FFXIVPlugin.Contract;
using XIVDeck.FFXIVPlugin.Game.Managers;
using XIVDeck.FFXIVPlugin.IoC;

namespace XIVDeck.FFXIVPlugin.Game.Watchers;

[Service(ServiceFlags.Singleton)]
public class HotbarWatcher : IDisposable {
    private readonly record struct SlotSnapshot(
        HotbarSlotType CommandType,
        uint CommandId,
        HotbarSlotType ApparentType,
        uint ApparentId,
        uint Cost);

    private readonly ILogger _log;
    private readonly IFramework _framework;
    private readonly IGameGui _gameGui;

    private readonly Lock _trackerLock = new();
    private readonly List<SlotWatchTracker> _trackers = [];
    private HotbarSlotRef[] _watched = [];

    private readonly Dictionary<int, SlotSnapshot> _snapshots = new();

    private bool _inventoryDirty;

    public HotbarWatcher(ILogger log, IFramework framework, IGameGui gameGui) {
        this._log = log;
        this._framework = framework;
        this._gameGui = gameGui;

        // AgentUpdate runs first in this case.
        this._gameGui.AgentUpdate += this.OnAgentUpdate;
        this._framework.Update += this.OnGameUpdate;
    }

    public SlotWatchTracker GetWatchTracker(Action<IReadOnlyList<HotbarSlotRef>> onChanged) {
        return new SlotWatchTracker(this, onChanged);
    }

    private void ApplyWatch(SlotWatchTracker slotWatchTracker, bool remove) {
        lock (this._trackerLock) {
            if (remove) {
                this._trackers.Remove(slotWatchTracker);
            } else if (!this._trackers.Contains(slotWatchTracker)) {
                this._trackers.Add(slotWatchTracker);
            }

            var watched = this._trackers
                .SelectMany(r => r.Slots)
                .Distinct()
                .ToArray();

            Volatile.Write(ref this._watched, watched);
        }
    }

    private void OnAgentUpdate(AgentUpdateFlag flags) {
        if ((flags & AgentUpdateFlag.InventoryUpdate) != 0) {
            this._inventoryDirty = true;
        }
    }

    private void OnGameUpdate(IFramework framework) {
        List<HotbarSlotRef>? changedSlots = null;
        foreach (var slot in Volatile.Read(ref this._watched)) {
            var index = (slot.HotbarId << 8) | slot.SlotId;
            var slotTracked = (this._snapshots.TryGetValue(index, out var previous));
            var current = GetSnapshot(slot, previous, this._inventoryDirty);
            if (slotTracked && (current == previous)) continue;

            this._snapshots[index] = current;

            changedSlots ??= [];
            changedSlots.Add(slot);
        }

        this._inventoryDirty = false;

        if (changedSlots != null) {
            Task.Run(() => this.PublishSlotUpdates(changedSlots));
        }
    }

    private void PublishSlotUpdates(List<HotbarSlotRef> changedSlots) {
        SlotWatchTracker[] registrations;
        lock (this._trackerLock) registrations = [.. this._trackers];

        foreach (var registration in registrations) {
            var slots = registration.Slots.Where(changedSlots.Contains).ToArray();
            if (slots.Length == 0) continue;

            try {
                registration.OnChanged(slots);
            } catch (Exception ex) {
                this._log.Error(ex, "Hotbar watcher callback failed");
            }
        }
    }

    private static unsafe SlotSnapshot GetSnapshot(HotbarSlotRef slot, SlotSnapshot? previous, bool refreshItemCosts) {
        var gameSlot = HotbarManager.GetSlotByIdFixed((uint)slot.HotbarId, (uint)slot.SlotId);
        HotbarManager.ResolveApparentAction(gameSlot, out var apparentType, out var apparentId);

        // Cost for items is particularly annoying to calculate, so we'll cache it unless we get a notification.
        uint cost;
        if (!refreshItemCosts && apparentType == HotbarSlotType.Item
                              && previous is { ApparentType: HotbarSlotType.Item } p
                              && p.ApparentId == apparentId) {
            cost = p.Cost;
        } else {
            cost = gameSlot->GetCostValueForSlot(apparentType, apparentId);
        }

        return new SlotSnapshot(gameSlot->CommandType, gameSlot->CommandId, apparentType, apparentId, cost);
    }

    public void Dispose() {
        this._gameGui.AgentUpdate -= this.OnAgentUpdate;
        this._framework.Update -= this.OnGameUpdate;
    }

    public sealed class SlotWatchTracker : IDisposable {
        private readonly HotbarWatcher _watcher;
        internal readonly Action<IReadOnlyList<HotbarSlotRef>> OnChanged;
        internal HotbarSlotRef[] Slots = [];

        internal SlotWatchTracker(HotbarWatcher watcher, Action<IReadOnlyList<HotbarSlotRef>> onChanged) {
            this._watcher = watcher;
            this.OnChanged = onChanged;
        }

        public void SetWatched(IEnumerable<HotbarSlotRef> slots) {
            this.Slots = [.. slots.Distinct()];
            this._watcher.ApplyWatch(this, false);
        }

        public void Dispose() {
            this._watcher.ApplyWatch(this, true);
        }
    }
}

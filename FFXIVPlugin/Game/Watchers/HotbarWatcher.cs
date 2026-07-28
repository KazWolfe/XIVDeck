using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
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
        uint ApparentId);

    private readonly ILogger _log;
    private readonly IFramework _framework;

    private readonly Lock _registrationLock = new();
    private readonly List<Registration> _registrations = [];

    private IReadOnlyDictionary<HotbarSlotRef, Registration[]> _watched =
        new Dictionary<HotbarSlotRef, Registration[]>();

    private readonly Dictionary<HotbarSlotRef, SlotSnapshot> _baselines = new();
    private IReadOnlyDictionary<HotbarSlotRef, Registration[]>? _baselinesFor;

    public HotbarWatcher(ILogger log, IFramework framework) {
        this._log = log;
        this._framework = framework;
        this._framework.Update += this.OnGameUpdate;
    }

    public Registration Register(Action<IReadOnlyList<HotbarSlotRef>> onChanged) {
        var registration = new Registration(this, onChanged);

        lock (this._registrationLock) {
            this._registrations.Add(registration);
        }

        return registration;
    }

    private void Update(Registration registration, IReadOnlySet<HotbarSlotRef>? slots) {
        lock (this._registrationLock) {
            if (slots == null) {
                this._registrations.Remove(registration);
            } else {
                registration.Slots = slots;
            }

            var watched = this._registrations
                .SelectMany(r => r.Slots, (r, slot) => (Slot: slot, Registration: r))
                .GroupBy(e => e.Slot, e => e.Registration)
                .ToDictionary(g => g.Key, g => g.ToArray());

            Volatile.Write(ref this._watched, watched);
        }
    }

    private unsafe void OnGameUpdate(IFramework framework) {
        var watched = Volatile.Read(ref this._watched);

        if (!ReferenceEquals(watched, this._baselinesFor)) {
            foreach (var slot in this._baselines.Keys.Where(slot => !watched.ContainsKey(slot)).ToList()) {
                this._baselines.Remove(slot);
            }

            this._baselinesFor = watched;
        }

        if (watched.Count == 0) return;

        Dictionary<Registration, List<HotbarSlotRef>>? changes = null;

        foreach (var (slot, registrations) in watched) {
            var gameSlot = HotbarManager.GetSlotByIdFixed((uint)slot.HotbarId, (uint)slot.SlotId);
            HotbarManager.ResolveApparentAction(gameSlot, out var apparentType, out var apparentId);
            var current = new SlotSnapshot(gameSlot->CommandType, gameSlot->CommandId, apparentType, apparentId);

            if (!this._baselines.TryGetValue(slot, out var previous)) {
                this._baselines[slot] = current;
                continue;
            }

            if (current == previous) continue;
            this._baselines[slot] = current;

            changes ??= new Dictionary<Registration, List<HotbarSlotRef>>();
            foreach (var registration in registrations) {
                if (!changes.TryGetValue(registration, out var list)) changes[registration] = list = [];
                list.Add(slot);
            }
        }

        if (changes == null) return;

        foreach (var (registration, slots) in changes) {
            try {
                registration.OnChanged(slots);
            } catch (Exception ex) {
                this._log.Error(ex, "Hotbar watcher callback failed");
            }
        }
    }

    public void Dispose() {
        this._framework.Update -= this.OnGameUpdate;
    }

    public sealed class Registration : IDisposable {
        private readonly HotbarWatcher _watcher;
        internal readonly Action<IReadOnlyList<HotbarSlotRef>> OnChanged;
        internal IReadOnlySet<HotbarSlotRef> Slots = new HashSet<HotbarSlotRef>();

        internal Registration(HotbarWatcher watcher, Action<IReadOnlyList<HotbarSlotRef>> onChanged) {
            this._watcher = watcher;
            this.OnChanged = onChanged;
        }

        public void SetSlots(IEnumerable<HotbarSlotRef> slots) {
            this._watcher.Update(this, slots.ToHashSet());
        }

        public void Dispose() {
            this._watcher.Update(this, null);
        }
    }
}

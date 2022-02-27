/*
 * Source: https://github.com/goaaats/Dalamud.FindAnything/blob/master/Dalamud.FindAnything/GameStateCache.cs
 */

#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Dalamud.Logging;
using Dalamud.Memory;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using Lumina.Excel.GeneratedSheets;

namespace FFXIVPlugin.Base {
    // borrowed from https://github.com/goaaats/Dalamud.FindAnything/blob/master/Dalamud.FindAnything/GameStateCache.cs
    public unsafe class GameStateCache {
        // E8 ?? ?? ?? ?? 84 C0 74 A4
        private delegate byte IsEmoteUnlockedDelegate(UIState* uiState, uint emoteId, byte unk);

        private readonly IsEmoteUnlockedDelegate? _isEmoteUnlocked;

        [return: MarshalAs(UnmanagedType.U1)]
        private delegate byte IsMountUnlockedDelegate(IntPtr mountBitmask, uint mountId);

        private readonly IsMountUnlockedDelegate? _isMountUnlocked;
        private readonly IntPtr _mountBitmask;
        private readonly byte* _minionBitmask = null;
        
        public struct Gearset {
            public int Slot { get; init; }
            public string Name { get; init; }
            public uint ClassJob { get; init; }
        }
        
        public IReadOnlyList<Emote>? UnlockedEmoteKeys { get; private set; }
        public IReadOnlyList<Mount>? UnlockedMountKeys { get; private set; }
        public IReadOnlyList<Companion>? UnlockedMinionKeys { get; private set; }
        public IReadOnlyList<Gearset>? Gearsets { get; private set; }

        internal bool IsMountUnlocked(uint mountId) {
            if (this._mountBitmask == IntPtr.Zero) {
                return false;
            }

            return this._isMountUnlocked!(this._mountBitmask, mountId) > 0;
        }

        internal bool IsMinionUnlocked(uint minionId) {
            if (this._minionBitmask == null) {
                return false;
            }

            return ((1 << ((int) minionId & 7)) & this._minionBitmask[minionId >> 3]) > 0;
        }

        private GameStateCache() {
            if (Injections.SigScanner.TryScanText("E8 ?? ?? ?? ?? 84 C0 74 A4", out var emoteUnlockedPtr)) {
                PluginLog.Verbose($"emoteUnlockedPtr: {emoteUnlockedPtr:X}");
                this._isEmoteUnlocked = Marshal.GetDelegateForFunctionPointer<IsEmoteUnlockedDelegate>(emoteUnlockedPtr);
            }

            Injections.SigScanner.TryGetStaticAddressFromSig(
                "48 8D 0D ?? ?? ?? ?? E8 ?? ?? ?? ?? 84 C0 74 5C 8B CB E8", out this._mountBitmask);
            PluginLog.Verbose($"mountBitmask: {this._mountBitmask:X}");

            if (Injections.SigScanner.TryScanText("E8 ?? ?? ?? ?? 84 C0 74 5C 8B CB",
                out var mountUnlockedPtr)) {
                PluginLog.Verbose($"mountUnlockedPtr: {mountUnlockedPtr:X}");
                this._isMountUnlocked = Marshal.GetDelegateForFunctionPointer<IsMountUnlockedDelegate>(mountUnlockedPtr);
            }

            if (Injections.SigScanner.TryGetStaticAddressFromSig(
                "48 8D 0D ?? ?? ?? ?? 0F B6 04 08 84 D0 75 10 B8 ?? ?? ?? ?? 48 8B 5C 24", out var minionBitmaskPtr)) {
                PluginLog.Verbose($"minionBitmaskPtr: {minionBitmaskPtr:X}");
                this._minionBitmask = (byte*) minionBitmaskPtr;
            }
        }

        public void Refresh() {
            if (this._isEmoteUnlocked != null) {
                var emotes = new List<Emote>();
                foreach (var emote in Injections.DataManager.GetExcelSheet<Emote>()!.Where(x => x.Order != 0)) {
                    if (emote.UnlockLink == 0 || this._isEmoteUnlocked(UIState.Instance(), emote.UnlockLink, 1) > 0) {
                        emotes.Add(emote);
                    }
                }

                UnlockedEmoteKeys = emotes;
            }

            if (this._isMountUnlocked != null) {
                UnlockedMountKeys = Injections.DataManager.GetExcelSheet<Mount>()!.Where(x => IsMountUnlocked(x.RowId)).ToList();
            }

            if (this._minionBitmask != null) {
                UnlockedMinionKeys = Injections.DataManager.GetExcelSheet<Companion>()!
                    .Where(x => IsMinionUnlocked(x.RowId)).ToList();
            }

            var gsModule = RaptureGearsetModule.Instance();
            var gearsets = new List<Gearset>();
            for (var i = 0; i < 100; i++) {
                var gs = gsModule->Gearset[i];

                if (gs == null || !gs->Flags.HasFlag(RaptureGearsetModule.GearsetFlag.Exists))
                    continue;

                var name = MemoryHelper.ReadString(new IntPtr(gs->Name), 47);

                gearsets.Add(new Gearset {
                    Slot = i + 1,
                    ClassJob = gs->ClassJob,
                    Name = name,
                });
            }

            this.Gearsets = gearsets;
        }

        public static GameStateCache Load() => new();
    }
}
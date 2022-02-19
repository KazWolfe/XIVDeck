/*
 * Source: https://github.com/goaaats/Dalamud.FindAnything/blob/master/Dalamud.FindAnything/GameStateCache.cs
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Dalamud.Logging;
using Dalamud.Memory;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using FFXIVClientStructs.FFXIV.Component.GUI;
using FFXIVPlugin.helpers;
using Lumina.Excel.GeneratedSheets;

namespace FFXIVPlugin.Base {
    // borrowed from https://github.com/goaaats/Dalamud.FindAnything/blob/master/Dalamud.FindAnything/GameStateCache.cs
    public unsafe class GameStateCache {
        // E8 ?? ?? ?? ?? 84 C0 74 A4
        private delegate byte IsEmoteUnlockedDelegate(UIState* uiState, uint emoteId, byte unk);

        private readonly IsEmoteUnlockedDelegate? isEmoteUnlocked;

        [return: MarshalAs(UnmanagedType.U1)]
        private delegate byte IsMountUnlockedDelegate(IntPtr mountBitmask, uint mountId);

        private readonly IsMountUnlockedDelegate? isMountUnlocked;
        private IntPtr mountBitmask;
        private byte* minionBitmask = null;
        
        public struct Gearset {
            public int Slot { get; set; }
            public string Name { get; set; }
            public uint ClassJob { get; set; }
        }
        
        public IReadOnlyList<Emote> UnlockedEmoteKeys { get; private set; }
        public IReadOnlyList<Mount> UnlockedMountKeys { get; private set; }
        public IReadOnlyList<Companion> UnlockedMinionKeys { get; private set; }
        public IReadOnlyList<Gearset> Gearsets { get; private set; }

        internal bool IsMountUnlocked(uint mountId) {
            if (this.mountBitmask == IntPtr.Zero) {
                return false;
            }

            return this.isMountUnlocked(this.mountBitmask, mountId) > 0;
        }

        internal bool IsMinionUnlocked(uint minionId) {
            if (this.minionBitmask == null) {
                return false;
            }

            return ((1 << ((int) minionId & 7)) & this.minionBitmask[minionId >> 3]) > 0;
        }

        private GameStateCache() {
            if (Injections.SigScanner.TryScanText("E8 ?? ?? ?? ?? 84 C0 74 A4", out var emoteUnlockedPtr)) {
                PluginLog.Verbose($"emoteUnlockedPtr: {emoteUnlockedPtr:X}");
                this.isEmoteUnlocked = Marshal.GetDelegateForFunctionPointer<IsEmoteUnlockedDelegate>(emoteUnlockedPtr);
            }

            Injections.SigScanner.TryGetStaticAddressFromSig(
                "48 8D 0D ?? ?? ?? ?? E8 ?? ?? ?? ?? 84 C0 74 5C 8B CB E8", out this.mountBitmask);
            PluginLog.Verbose($"mountBitmask: {this.mountBitmask:X}");

            if (Injections.SigScanner.TryScanText("E8 ?? ?? ?? ?? 84 C0 74 5C 8B CB",
                out var mountUnlockedPtr)) {
                PluginLog.Verbose($"mountUnlockedPtr: {mountUnlockedPtr:X}");
                this.isMountUnlocked = Marshal.GetDelegateForFunctionPointer<IsMountUnlockedDelegate>(mountUnlockedPtr);
            }

            if (Injections.SigScanner.TryGetStaticAddressFromSig(
                "48 8D 0D ?? ?? ?? ?? 0F B6 04 08 84 D0 75 10 B8 ?? ?? ?? ?? 48 8B 5C 24", out var minionBitmaskPtr)) {
                PluginLog.Verbose($"minionBitmaskPtr: {minionBitmaskPtr:X}");
                this.minionBitmask = (byte*) minionBitmaskPtr;
            }
        }

        public void Refresh() {
            if (this.isEmoteUnlocked != null) {
                var emotes = new List<Emote>();
                foreach (var emote in Injections.DataManager.GetExcelSheet<Emote>()!.Where(x => x.Order != 0)) {
                    if (emote.UnlockLink == 0 || this.isEmoteUnlocked(UIState.Instance(), emote.UnlockLink, 1) > 0) {
                        emotes.Add(emote);
                    }
                }

                UnlockedEmoteKeys = emotes;
            }

            if (this.isMountUnlocked != null) {
                UnlockedMountKeys = Injections.DataManager.GetExcelSheet<Mount>()!.Where(x => IsMountUnlocked(x.RowId)).ToList();
            }

            if (this.minionBitmask != null) {
                UnlockedMinionKeys = Injections.DataManager.GetExcelSheet<Companion>()!
                    .Where(x => IsMinionUnlocked(x.RowId)).ToList();
            }

            var gsModule = RaptureGearsetModule.Instance();
            var cj = Injections.DataManager.GetExcelSheet<ClassJob>()!;
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

            Gearsets = gearsets;

            PluginLog.LogVerbose($"{UnlockedEmoteKeys.Count} emotes unlocked.");
        }

        public static GameStateCache Load() => new GameStateCache();
    }
}
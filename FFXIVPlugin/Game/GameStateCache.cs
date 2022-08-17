#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Memory;
using Dalamud.Utility.Signatures;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using Lumina.Excel.GeneratedSheets;
using XIVDeck.FFXIVPlugin.Base;

namespace XIVDeck.FFXIVPlugin.Game; 

// borrowed from https://github.com/goaaats/Dalamud.FindAnything/blob/master/Dalamud.FindAnything/GameStateCache.cs
// ... and then promptly adapted and destroyed. I am sorry, goat.
internal unsafe class GameStateCache {
    private static class Signatures {
        internal const string MinionBitmask = "48 8D 0D ?? ?? ?? ?? 0F B6 04 08 84 D0 75 10 B8 ?? ?? ?? ?? 48 8B 5C 24";
        internal const string PlayerState = "48 8D 0D ?? ?? ?? ?? E9 ?? ?? ?? ?? CC 40 53";
    
        internal const string IsMountUnlocked = "E8 ?? ?? ?? ?? 84 C0 74 5C 8B CB";
        internal const string IsOrnamentUnlocked = "E8 ?? ?? ?? ?? BA ?? ?? ?? ?? 41 0F B6 CE";
        internal const string IsEmoteUnlocked = "E8 ?? ?? ?? ?? 84 C0 74 A4";
        
        // This sig is (interestingly) hand-found in order to allow Square to change McGuffin counts without
        // breaking my code. Simple explanation is that ?? is current count of McGuffins.
        internal const string IsMcGuffinUnlocked = "8D 42 FF 3C ?? 77 44 4C 8B 89";
    }
        
    internal struct Gearset {
        public int Slot { get; init; }
        public string Name { get; init; }
        public uint ClassJob { get; init; }
    }
        
    // METHODS //
    [Signature(Signatures.IsEmoteUnlocked, Fallibility = Fallibility.Fallible)]
    private readonly delegate* unmanaged<UIState*, uint, byte, byte> _isEmoteUnlocked = null;

    [Signature(Signatures.IsMountUnlocked, Fallibility = Fallibility.Fallible)]
    private readonly delegate* unmanaged<IntPtr, uint, byte> _isMountUnlocked = null;

    [Signature(Signatures.IsOrnamentUnlocked, Fallibility = Fallibility.Fallible)]
    private readonly delegate* unmanaged<IntPtr, uint, byte> _isOrnamentUnlocked = null;

    [Signature(Signatures.IsMcGuffinUnlocked, Fallibility = Fallibility.Fallible)]
    private readonly delegate* unmanaged<IntPtr, uint, byte> _isMcGuffinUnlocked = null;
        
    // Fields //
    [Signature(Signatures.PlayerState, ScanType = ScanType.StaticAddress)]
    private readonly IntPtr? _playerState = null;
        
    [Signature(Signatures.MinionBitmask, ScanType = ScanType.StaticAddress)]
    private readonly IntPtr? _minionBitmask = null;

    internal IReadOnlyList<Emote>? UnlockedEmotes { get; private set; }
    internal IReadOnlyList<Mount>? UnlockedMounts { get; private set; }
    internal IReadOnlyList<Companion>? UnlockedMinions { get; private set; }
    internal IReadOnlyList<Ornament>? UnlockedOrnaments { get; private set; }
    internal IReadOnlyList<Gearset>? Gearsets { get; private set; }

    internal bool IsEmoteUnlocked(uint emoteId) {
        if (this._isEmoteUnlocked == null) return false;
            
        // works around showing emotes when nobody is logged in, as we check unlockLink 0 
        if (!Injections.ClientState.IsLoggedIn) return false;

        var emote = Injections.DataManager.Excel.GetSheet<Emote>()!.GetRow(emoteId);
        if (emote == null || emote.Order == 0) return false;

        return emote.UnlockLink == 0 || (this._isEmoteUnlocked(UIState.Instance(), emote.UnlockLink, 1) > 0);
    }
    
    internal bool IsMountUnlocked(uint mountId) {
        if (this._playerState == null || this._playerState.Value == IntPtr.Zero) {
            return false;
        }

        return this._isMountUnlocked(this._playerState.Value, mountId) > 0;
    }
    
    internal bool IsMinionUnlocked(uint minionId) {
        if (this._minionBitmask == null || this._minionBitmask.Value == IntPtr.Zero) {
            return false;
        }

        return ((1 << ((int) minionId & 7)) & ((byte*) this._minionBitmask.Value)[minionId >> 3]) > 0;
    }
    
    internal bool IsOrnamentUnlocked(uint ornamentId) {
        if (this._playerState == null || this._playerState.Value == IntPtr.Zero) {
            return false;
        }

        return this._isOrnamentUnlocked(this._playerState.Value, ornamentId) > 0;
    }
    
    internal bool IsMcGuffinUnlocked(uint mcguffinId) {
        if (this._playerState == null || this._playerState.Value == IntPtr.Zero) {
            return false;
        }
            
        return this._isMcGuffinUnlocked(this._playerState.Value, (byte) mcguffinId) > 0;
    }
    
    internal GameStateCache() {
        SignatureHelper.Initialise(this);
        this.Refresh();
    }

    internal void Refresh() {
        if (this._isEmoteUnlocked != null) {
            this.UnlockedEmotes = Injections.DataManager.GetExcelSheet<Emote>()!
                .Where(x => this.IsEmoteUnlocked(x.RowId)).ToList();
        }

        if (this._isMountUnlocked != null) {
            this.UnlockedMounts = Injections.DataManager.GetExcelSheet<Mount>()!
                .Where(x => this.IsMountUnlocked(x.RowId)).ToList();
        }

        if (this._minionBitmask == null || this._minionBitmask.Value != IntPtr.Zero) {
            this.UnlockedMinions = Injections.DataManager.GetExcelSheet<Companion>()!
                .Where(x => this.IsMinionUnlocked(x.RowId)).ToList();
        }

        if (this._isOrnamentUnlocked != null) {
            this.UnlockedOrnaments = Injections.DataManager.GetExcelSheet<Ornament>()!
                .Where(x => this.IsOrnamentUnlocked(x.RowId)).ToList();
        }
        
        var gearsets = new List<Gearset>();
        for (var i = 0; i < 100; i++) {
            var gs = RaptureGearsetModule.Instance()->Gearset[i];

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
}
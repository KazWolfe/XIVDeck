using System;
using System.Collections.Generic;
using Dalamud.Logging;
using Dalamud.Memory;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using FFXIVClientStructs.FFXIV.Common.Configuration;
using XIVDeck.FFXIVPlugin.Utils;

namespace XIVDeck.FFXIVPlugin.Game;

// Shamelessly stolen from SimpleTweaks, again. Thank you for the work, Cara!
// https://github.com/Caraxi/SimpleTweaksPlugin/blob/main/Utility/GameConfig.cs
public static unsafe class GameConfig {
    public class GameConfigSection {
        private readonly Dictionary<string, uint> _indexCache = new();
        private readonly ConfigBase* _configBase;
        
        // Note for future me: This class operates on the *names* of keys, not the values as defined by the enum.
        // This may be confusing and honestly I don't like the fact that I do it this way, but I like the correctness
        // guarantee, so... this is what we get. 

        public GameConfigSection(ConfigBase* configBase) {
            this._configBase = configBase;

            // Preload cache for performance
            var e = this._configBase->ConfigEntry;
            for (var i = 0U; i < this._configBase->ConfigCount; i++, e++) {
                if (e->Name == null) continue;
                this._indexCache[MemoryHelper.ReadStringNullTerminated((nint) e->Name)] = i;
            }
        }

        private bool TryGetIndex(string name, out uint index) {
            if (this._indexCache.TryGetValue(name, out index)) return true;

            PluginLog.Verbose($"Cache miss on TryGetIndex for {name}!");
            var e = this._configBase->ConfigEntry;
            for (var i = 0U; i < this._configBase->ConfigCount; i++, e++) {
                if (e->Name == null) continue;
                var eName = MemoryHelper.ReadStringNullTerminated((nint) e->Name);

                if (eName.Equals(name)) {
                    this._indexCache.Add(name, i);
                    index = i;
                    return true;
                }
            }

            return false;
        }

        private bool TryGetEntry(uint index, out ConfigEntry* entry) {
            entry = null;
            if (this._configBase->ConfigEntry == null || index >= this._configBase->ConfigCount) return false;
            entry = this._configBase->ConfigEntry;
            entry += index;
            return true;
        }

        private bool TryGetEntry(ConfigOption option, out ConfigEntry* entry, bool searchByName = true) {
            entry = null;
            var index = (uint) option;
            if (searchByName && !this.TryGetIndex(option.ToString(), out index)) return false;

            return this.TryGetEntry(index, out entry);
        }

        private bool TryGetEntry(string option, out ConfigEntry* entry) {
            entry = null;
            return this.TryGetIndex(option, out var index) && this.TryGetEntry(index, out entry);
        }

        public bool TryGetBool(string option, out bool value) {
            value = false;
            if (!this.TryGetEntry(option, out var entry)) return false;
            value = entry->Value.UInt != 0;
            return true;
        }

        public bool GetBool(string option) {
            if (!this.TryGetBool(option, out var value))
                throw new ArgumentOutOfRangeException(nameof(option), @$"No option {option} was found.");

            return value;
        }

        public void Set(string option, bool value) {
            if (!this.TryGetEntry(option, out var entry)) return;
            entry->SetValue(value ? 1U : 0U);
        }

        public bool TryGetUInt(string option, out uint value) {
            value = 0;
            if (!this.TryGetEntry(option, out var entry)) return false;
            value = entry->Value.UInt;
            return true;
        }

        public uint GetUInt(string option) {
            if (!this.TryGetUInt(option, out var value))
                throw new ArgumentOutOfRangeException(nameof(option), @$"No option {option} was found.");

            return value;
        }

        public void Set(string option, uint value) {
            if (!this.TryGetEntry(option, out var entry)) return;
            entry->SetValue(value);
        }

        public IDisposable TemporarySet(string option, bool value) {
            var oldValue = this.GetBool(option);

            this.Set(option, value);
            return new DisposableWrapper(() => { this.Set(option, oldValue); });
        }
    }

    static GameConfig() {
        System = new GameConfigSection(&Framework.Instance()->SystemConfig.CommonSystemConfig.ConfigBase);
        UiConfig = new GameConfigSection(&Framework.Instance()->SystemConfig.CommonSystemConfig.UiConfig);
        UiControl = new GameConfigSection(&Framework.Instance()->SystemConfig.CommonSystemConfig.UiControlConfig);
    }


    public static readonly GameConfigSection System;
    public static readonly GameConfigSection UiConfig;
    public static readonly GameConfigSection UiControl;
}
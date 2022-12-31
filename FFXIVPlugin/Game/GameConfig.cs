using System;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using FFXIVClientStructs.FFXIV.Common.Configuration;
using XIVDeck.FFXIVPlugin.Utils;

namespace XIVDeck.FFXIVPlugin.Game; 

// Shamelessly stolen from SimpleTweaks, again. Thank you for the work, Cara!
// https://github.com/Caraxi/SimpleTweaksPlugin/blob/main/Utility/GameConfig.cs
public static unsafe class GameConfig {

    private static readonly SigHelper SigHelper = XIVDeckPlugin.Instance.SigHelper;

    public class GameConfigSection {

        private readonly ConfigBase* _configBase;
        
        public GameConfigSection(ConfigBase* configBase) {
            this._configBase = configBase;
        }

        private bool TryGetEntry(uint index, out ConfigEntry* entry) {
            entry = null;
            if (this._configBase->ConfigEntry == null || index >= this._configBase->ConfigCount) return false;
            entry = this._configBase->ConfigEntry;
            entry += index;
            return true;
        }
        
        public bool TryGetBool(ConfigOption option, out bool value) {
            value = false;
            if (!this.TryGetEntry((uint) option, out var entry)) return false;
            value = entry->Value.UInt != 0;
            return true;
        }

        public bool GetBool(ConfigOption option) {
            if (!this.TryGetBool(option, out var value)) 
                throw new Exception($"Failed to get Bool '{nameof(option)}'");
            
            return value;
        }

        public void Set(ConfigOption option, bool value) {
            if (!this.TryGetEntry((uint) option, out var entry)) return;
            SigHelper.SetConfigValueUInt(entry, value ? 1U : 0U);
        }

        public bool TryGetUInt(ConfigOption option, out uint value) {
            value = 0;
            if (!this.TryGetEntry((uint) option, out var entry)) return false;
            value = entry->Value.UInt;
            return true;
        }

        public uint GetUInt(ConfigOption option) {
            if (!this.TryGetUInt(option, out var value)) 
                throw new Exception($"Failed to get UInt '{nameof(option)}'");
            
            return value;
        }

        public void Set(ConfigOption option, uint value) {
            if (!this.TryGetEntry((uint) option, out var entry)) return;
            SigHelper.SetConfigValueUInt(entry, value);
        }

        public IDisposable? TemporarySet(ConfigOption option, bool? value) {
            if (value == null) return null;

            var oldValue = this.GetBool(option);

            return new DisposableWrapper(delegate {
                this.Set(option, value.Value);
            }, delegate {
                this.Set(option, oldValue);
            });
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

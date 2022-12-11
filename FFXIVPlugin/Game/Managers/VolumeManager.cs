using System.Collections.Generic;
using Dalamud.Logging;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using XIVDeck.FFXIVPlugin.Game.Data;

namespace XIVDeck.FFXIVPlugin.Game.Managers;

public static class VolumeManager {
    public static readonly Dictionary<SoundChannel, (ConfigOption Level, ConfigOption MuteState)> Channels = new() {
        {SoundChannel.Master, (ConfigOption.SoundMaster, ConfigOption.IsSndMaster)},
        {SoundChannel.BackgroundMusic, (ConfigOption.SoundBgm, ConfigOption.IsSndBgm)},
        {SoundChannel.SoundEffects, (ConfigOption.SoundSe, ConfigOption.IsSndSe)},
        {SoundChannel.Voice, (ConfigOption.SoundVoice, ConfigOption.IsSndVoice)},
        {SoundChannel.System, (ConfigOption.SoundSystem, ConfigOption.IsSndSystem)},
        {SoundChannel.Ambient, (ConfigOption.SoundEnv, ConfigOption.IsSndEnv)},
        {SoundChannel.Performance, (ConfigOption.SoundPerform, ConfigOption.IsSndPerform)}
    };

    public static uint GetVolume(SoundChannel channel) {
        return GameConfig.System.GetUInt(Channels[channel].Level);
    }

    public static bool IsMuted(SoundChannel channel) {
        return GameConfig.System.GetUInt(Channels[channel].MuteState) == 1u;
    }

    public static void SetVolume(SoundChannel channel, uint volume) {   
        if (volume > 100) {
            volume = 100;
        }

        PluginLog.Debug($"Setting volume of channel {channel.ToString()} to {volume}.");
        GameConfig.System.Set(Channels[channel].Level, volume);
    }

    public static void SetMuted(SoundChannel channel, bool muted) {
        PluginLog.Debug($"Setting mute state of channel {channel.ToString()} to {muted}");
        GameConfig.System.Set(Channels[channel].MuteState, muted);
    }
}
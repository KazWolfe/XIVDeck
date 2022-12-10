using System;
using System.Collections.Generic;
using Dalamud.Game;
using Dalamud.Logging;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using XIVDeck.FFXIVPlugin.Base;
using XIVDeck.FFXIVPlugin.Game.Data;
using XIVDeck.FFXIVPlugin.Server;
using XIVDeck.FFXIVPlugin.Server.Messages.Outbound;

namespace XIVDeck.FFXIVPlugin.Game;

public class VolumeManager : IDisposable {
    private static readonly Dictionary<SoundChannel, (ConfigOption Level, ConfigOption MuteState)> Channels = new() {
        {SoundChannel.Master, (ConfigOption.SoundMaster, ConfigOption.IsSndMaster)},
        {SoundChannel.BackgroundMusic, (ConfigOption.SoundBgm, ConfigOption.IsSndBgm)},
        {SoundChannel.SoundEffects, (ConfigOption.SoundSe, ConfigOption.IsSndSe)},
        {SoundChannel.Voice, (ConfigOption.SoundVoice, ConfigOption.IsSndVoice)},
        {SoundChannel.System, (ConfigOption.SoundSystem, ConfigOption.IsSndSystem)},
        {SoundChannel.Ambient, (ConfigOption.SoundEnv, ConfigOption.IsSndEnv)},
        {SoundChannel.Performance, (ConfigOption.SoundPerform, ConfigOption.IsSndPerform)}
    };

    private readonly Dictionary<SoundChannel, (int Level, bool Muted)> _volumeCache = new();

    public VolumeManager() {
        Injections.Framework.Update += this.OnGameUpdate;
    }

    public void Dispose() {
        Injections.Framework.Update -= this.OnGameUpdate;

        GC.SuppressFinalize(this);
    }

    private void OnGameUpdate(Framework framework) {
        foreach (var (channel, (levelOption, muteOption)) in Channels) {
            var volumeLevel = GameConfig.System.GetUInt(levelOption);
            var muted = GameConfig.System.GetBool(muteOption);

            if (!this._volumeCache.TryGetValue(channel, out var result)) {
                result = (-1, false);
            }

            if (result.Level == volumeLevel && result.Muted == muted) continue;

            result = ((int) volumeLevel, muted);
            PluginLog.Verbose($"Volume update for channel {channel.ToString()}. " +
                              $"Vol = {volumeLevel} Muted = {muted}");
            XIVDeckWSServer.Instance?.BroadcastMessage(new WSVolumeUpdateMessage(channel, volumeLevel, muted));

            this._volumeCache[channel] = result;
        }
    }

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

        PluginLog.Debug($"Volume update - {channel.ToString()} to {volume}.");
        GameConfig.System.Set(Channels[channel].Level, volume);
    }

    public static void SetMuted(SoundChannel channel, bool muted) {
        PluginLog.Debug($"Setting mute state of {channel.ToString()} to {muted}");
        GameConfig.System.Set(Channels[channel].MuteState, muted);
    }
}
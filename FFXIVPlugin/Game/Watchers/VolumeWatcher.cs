using System;
using System.Collections.Generic;
using Dalamud.Game;
using Dalamud.Logging;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using XIVDeck.FFXIVPlugin.Base;
using XIVDeck.FFXIVPlugin.Game.Data;
using XIVDeck.FFXIVPlugin.Server;
using XIVDeck.FFXIVPlugin.Server.Messages.Outbound;

namespace XIVDeck.FFXIVPlugin.Game.Watchers; 

public class VolumeWatcher : IDisposable {
    public static readonly Dictionary<SoundChannel, (ConfigOption Level, ConfigOption MuteState)> Channels = new() {
        {SoundChannel.Master, (ConfigOption.SoundMaster, ConfigOption.IsSndMaster)},
        {SoundChannel.BackgroundMusic, (ConfigOption.SoundBgm, ConfigOption.IsSndBgm)},
        {SoundChannel.SoundEffects, (ConfigOption.SoundSe, ConfigOption.IsSndSe)},
        {SoundChannel.Voice, (ConfigOption.SoundVoice, ConfigOption.IsSndVoice)},
        {SoundChannel.System, (ConfigOption.SoundSystem, ConfigOption.IsSndSystem)},
        {SoundChannel.Ambient, (ConfigOption.SoundEnv, ConfigOption.IsSndEnv)},
        {SoundChannel.Performance, (ConfigOption.SoundPerform, ConfigOption.IsSndPerform)}
    };
    
    // internal cache of volume states for known update
    private readonly Dictionary<SoundChannel, (uint Level, bool Muted)> _volumeCache = new();
    
    // list of changes to apply
    private Dictionary<SoundChannel, (uint? RequestedLevel, bool? RequestedMute)> _enqueuedChanges = new();
    
    public VolumeWatcher() {
        Injections.Framework.Update += this.OnGameUpdate;
    }

    public void Dispose() {
        Injections.Framework.Update -= this.OnGameUpdate;

        GC.SuppressFinalize(this);
    }

    private void OnGameUpdate(Framework framework) {
        foreach (var channel in Channels.Keys) {
            var volumeLevel = GetVolumeRaw(channel);
            var muted = IsMutedRaw(channel);

            if (this._enqueuedChanges.TryGetValue(channel, out var enqueued)) {
                if (enqueued.RequestedLevel != null) {
                    volumeLevel = enqueued.RequestedLevel.Value;
                    SetVolume(channel, volumeLevel);
                }

                if (enqueued.RequestedMute != null) {
                    muted = enqueued.RequestedMute.Value;
                    SetMute(channel, muted);
                }

                this._enqueuedChanges.Remove(channel);
            }
            
            var result = this._volumeCache.GetValueOrDefault(channel);
            if (result.Level == volumeLevel && result.Muted == muted) continue;

            result = (volumeLevel, muted);
            PluginLog.Verbose($"Volume update for channel {channel.ToString()}. " +
                              $"Vol = {volumeLevel} Muted = {muted}");
            XIVDeckWSServer.Instance?.BroadcastMessage(new WSVolumeUpdateMessage(channel, volumeLevel, muted));
            
            this._volumeCache[channel] = result;
        }
    }

    public uint GetVolume(SoundChannel channel) {
        return this._enqueuedChanges.GetValueOrDefault(channel).RequestedLevel ?? GetVolumeRaw(channel);
    }

    public bool IsMuted(SoundChannel channel) {
        return this._enqueuedChanges.GetValueOrDefault(channel).RequestedMute ?? IsMutedRaw(channel);
    }

    public void EnqueueVolumeChange(SoundChannel channel, uint volume) {
        if (volume > 100) volume = 100;

        var change = this._enqueuedChanges.GetValueOrDefault(channel);
        
        if (change.RequestedLevel != null)
            PluginLog.Warning($"Requested change to channel {channel} while one was already enqueued.\n" +
                              $"    Old: {change.RequestedLevel}  New: {volume}");
        
        change.RequestedLevel = volume;
        this._enqueuedChanges[channel] = change;
    }

    public void EnqueueMute(SoundChannel channel, bool muted) {
        var change = this._enqueuedChanges.GetValueOrDefault(channel);

        if (change.RequestedMute != null)
            PluginLog.Warning($"Requested change to channel {channel} while one was already enqueued.\n" +
                              $"    Old: {change.RequestedMute}  New: {muted}");

        change.RequestedMute = muted;
        this._enqueuedChanges[channel] = change;
    }

    private static uint GetVolumeRaw(SoundChannel channel) {
        return GameConfig.System.GetUInt(Channels[channel].Level);
    }

    private static bool IsMutedRaw(SoundChannel channel) {
        return GameConfig.System.GetUInt(Channels[channel].MuteState) == 1u;
    }

    private static void SetVolume(SoundChannel channel, uint level) {
        GameConfig.System.Set(Channels[channel].Level, level);
    }

    private static void SetMute(SoundChannel channel, bool muted) {
        GameConfig.System.Set(Channels[channel].MuteState, muted);
    }
}
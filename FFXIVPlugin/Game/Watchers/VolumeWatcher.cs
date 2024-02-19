using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dalamud.Game.Config;
using XIVDeck.FFXIVPlugin.Base;
using XIVDeck.FFXIVPlugin.Game.Data;
using XIVDeck.FFXIVPlugin.Server.Messages.Outbound;

namespace XIVDeck.FFXIVPlugin.Game.Watchers;

public class VolumeWatcher : IDisposable {
    private static readonly Dictionary<SoundChannel, (string Level, string MuteState)> Channels = new() {
        {SoundChannel.Master, ("SoundMaster", "IsSndMaster")},
        {SoundChannel.BackgroundMusic, ("SoundBgm", "IsSndBgm")},
        {SoundChannel.SoundEffects, ("SoundSe", "IsSndSe")},
        {SoundChannel.Voice, ("SoundVoice", "IsSndVoice")},
        {SoundChannel.System, ("SoundSystem", "IsSndSystem")},
        {SoundChannel.Ambient, ("SoundEnv", "IsSndEnv")},
        {SoundChannel.Performance, ("SoundPerform", "IsSndPerform")}
    };

    // cursed.
    private static readonly Dictionary<string, SoundChannel> ReverseChannels = BuildReverseMap();

    private readonly ConcurrentDictionary<SoundChannel, (uint? RequestedLevel, bool? RequestedMute)> _enqueuedChanges = new();
    private Task? _updateTask;

    public VolumeWatcher() {
        Injections.GameConfig.SystemChanged += this.OnConfigChange;
    }

    public void Dispose() {
        Injections.GameConfig.SystemChanged -= this.OnConfigChange;

        GC.SuppressFinalize(this);
    }
    
    public uint GetVolume(SoundChannel channel) {
        return this._enqueuedChanges.GetValueOrDefault(channel).RequestedLevel ?? GetVolumeRaw(channel);
    }

    public bool IsMuted(SoundChannel channel) {
        return this._enqueuedChanges.GetValueOrDefault(channel).RequestedMute ?? IsMutedRaw(channel);
    }

    private void OnConfigChange(object? sender, ConfigChangeEvent ev) {
        if (!ReverseChannels.TryGetValue(ev.Option.ToString(), out var targetChannel)) return;

        var channelMuted = IsMutedRaw(targetChannel);
        var channelVolume = GetVolumeRaw(targetChannel);

        var message = new WSVolumeUpdateMessage(targetChannel, channelVolume, channelMuted);

        try {
            XIVDeckPlugin.Instance.Server.BroadcastMessage(message);
        } catch (Exception ex) {
            Injections.PluginLog.Error(ex, "Could not dispatch volume update message!");
        }
    }

    public void EnqueueVolumeChange(SoundChannel channel, uint volume) {
        if (volume > 100) volume = 100;

        var change = this._enqueuedChanges.GetValueOrDefault(channel);

        if (change.RequestedLevel != null)
            Injections.PluginLog.Warning($"Requested change to channel {channel} while one was already enqueued.\n" +
                              $"    Old: {change.RequestedLevel}  New: {volume}");

        change.RequestedLevel = volume;
        this._enqueuedChanges[channel] = change;
        
        this.ScheduleUpdate();
    }

    public void EnqueueMute(SoundChannel channel, bool muted) {
        var change = this._enqueuedChanges.GetValueOrDefault(channel);

        if (change.RequestedMute != null)
            Injections.PluginLog.Warning($"Requested change to channel {channel} while one was already enqueued.\n" +
                              $"    Old: {change.RequestedMute}  New: {muted}");

        change.RequestedMute = muted;
        this._enqueuedChanges[channel] = change;
        
        this.ScheduleUpdate();
    }

    private void ScheduleUpdate() {
        if (this._updateTask is {IsCompleted: false}) return;

        this._updateTask = Injections.Framework.RunOnFrameworkThread(() => {
            foreach (var (channel, enqueued) in this._enqueuedChanges) {
                if (enqueued.RequestedLevel != null) {
                    Injections.GameConfig.System.Set(Channels[channel].Level, enqueued.RequestedLevel.Value);
                }

                if (enqueued.RequestedMute != null) {
                    Injections.GameConfig.System.Set(Channels[channel].MuteState, enqueued.RequestedMute.Value);
                }
            }
            
            this._enqueuedChanges.Clear();
        });
    }

    private static uint GetVolumeRaw(SoundChannel channel) {
        return Injections.GameConfig.System.GetUInt(Channels[channel].Level);
    }

    private static bool IsMutedRaw(SoundChannel channel) {
        return Injections.GameConfig.System.GetBool(Channels[channel].MuteState);
    }

    private static Dictionary<string, SoundChannel> BuildReverseMap() {
        var result = new Dictionary<string, SoundChannel>();

        foreach (var (channel, (levelConfig, muteConfig)) in Channels) {
            result[levelConfig] = channel;
            result[muteConfig] = channel;
        }

        return result;
    }
}
using System;
using System.Collections.Generic;
using System.Threading;
using Dalamud.Game.Config;
using Dalamud.Plugin.Services;
using Serilog;
using XIVDeck.FFXIVPlugin.IoC;
using XIVDeck.FFXIVPlugin.Utils;
using XIVDeck.FFXIVPlugin.Contract;

namespace XIVDeck.FFXIVPlugin.Game.Managers;

[Service(ServiceFlags.Singleton)]
public class VolumeManager : IDisposable {
    private static readonly Dictionary<SoundChannel, (string Level, string MuteState)> Channels = new() {
        { SoundChannel.Master, ("SoundMaster", "IsSndMaster") },
        { SoundChannel.BackgroundMusic, ("SoundBgm", "IsSndBgm") },
        { SoundChannel.SoundEffects, ("SoundSe", "IsSndSe") },
        { SoundChannel.Voice, ("SoundVoice", "IsSndVoice") },
        { SoundChannel.System, ("SoundSystem", "IsSndSystem") },
        { SoundChannel.Ambient, ("SoundEnv", "IsSndEnv") },
        { SoundChannel.Performance, ("SoundPerform", "IsSndPerform") }
    };

    // cursed.
    private static readonly Dictionary<string, SoundChannel> ReverseChannels = BuildReverseMap();

    private readonly Lock _updateLock = new();
    private readonly Dictionary<SoundChannel, (uint? RequestedLevel, bool? RequestedMute)> _enqueuedChanges = new();
    private bool _updateScheduled;

    public event EventHandler<VolumeState>? VolumeChanged;

    private readonly ILogger _log;
    private readonly IGameConfig _gameConfig;
    private readonly IFramework _framework;

    public VolumeManager(ILogger logger, IGameConfig gameConfig, IFramework framework) {
        this._log = logger;
        this._gameConfig = gameConfig;
        this._framework = framework;
        this._gameConfig.SystemChanged += this.OnConfigChange;
    }

    public void Dispose() {
        this._gameConfig.SystemChanged -= this.OnConfigChange;

        GC.SuppressFinalize(this);
    }

    public uint GetVolume(SoundChannel channel) {
        lock (this._updateLock) {
            return this._enqueuedChanges.GetValueOrDefault(channel).RequestedLevel ?? this.GetVolumeRaw(channel);
        }
    }

    /// <summary>The channel's state, including any change that's enqueued but not yet applied.</summary>
    public VolumeState GetState(SoundChannel channel) {
        return new VolumeState {
            Channel = channel,
            Volume = (int)this.GetVolume(channel),
            Muted = this.IsMuted(channel),
        };
    }

    public bool IsMuted(SoundChannel channel) {
        lock (this._updateLock) {
            return this._enqueuedChanges.GetValueOrDefault(channel).RequestedMute ?? this.IsMutedRaw(channel);
        }
    }

    private void OnConfigChange(object? sender, ConfigChangeEvent ev) {
        if (!ReverseChannels.TryGetValue(ev.Option.ToString(), out var targetChannel)) return;

        this.VolumeChanged?.InvokeSafely(this, new VolumeState {
            Channel = targetChannel,
            Volume = (int)this.GetVolumeRaw(targetChannel),
            Muted = this.IsMutedRaw(targetChannel),
        });
    }

    public void EnqueueVolumeChange(SoundChannel channel, uint volume) {
        if (volume > 100) volume = 100; // guard - let's not make the game upset.

        lock (this._updateLock) {
            var change = this._enqueuedChanges.GetValueOrDefault(channel);

            if (change.RequestedLevel != null)
                this._log.Debug(
                    "Requested change to channel {Channel} while one was already enqueued.\n" +
                    "    Old: {OldVolume}  New: {NewVolume}", channel, change.RequestedLevel, volume);

            change.RequestedLevel = volume;
            this._enqueuedChanges[channel] = change;
        }

        this.ScheduleUpdate();
    }

    public void EnqueueMute(SoundChannel channel, bool muted) {
        lock (this._updateLock) {
            var change = this._enqueuedChanges.GetValueOrDefault(channel);

            if (change.RequestedMute != null)
                this._log.Debug("Requested change to channel {Channel} while one was already enqueued.\n" +
                                "    Old: {OldMuted}  New: {NewMuted}", channel, change.RequestedMute, muted);

            change.RequestedMute = muted;
            this._enqueuedChanges[channel] = change;
        }

        this.ScheduleUpdate();
    }

    private void ScheduleUpdate() {
        lock (this._updateLock) {
            if (this._updateScheduled) return;
            this._updateScheduled = true;
        }

        _ = this._framework.RunOnFrameworkThread(this.ApplyEnqueuedChanges);
    }

    private void ApplyEnqueuedChanges() {
        Dictionary<SoundChannel, (uint? RequestedLevel, bool? RequestedMute)> changes;
        lock (this._updateLock) {
            changes = new Dictionary<SoundChannel, (uint?, bool?)>(this._enqueuedChanges);
            this._enqueuedChanges.Clear();
            this._updateScheduled = false;
        }

        foreach (var (channel, enqueued) in changes) {
            if (enqueued.RequestedLevel != null) {
                this._gameConfig.System.Set(Channels[channel].Level, enqueued.RequestedLevel.Value);
            }

            if (enqueued.RequestedMute != null) {
                this._gameConfig.System.Set(Channels[channel].MuteState, enqueued.RequestedMute.Value);
            }
        }
    }

    private uint GetVolumeRaw(SoundChannel channel) {
        return this._gameConfig.System.GetUInt(Channels[channel].Level);
    }

    private bool IsMutedRaw(SoundChannel channel) {
        return this._gameConfig.System.GetBool(Channels[channel].MuteState);
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

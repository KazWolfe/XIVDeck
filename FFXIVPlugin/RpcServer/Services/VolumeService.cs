using System;
using System.Collections.Generic;
using XIVDeck.FFXIVPlugin.Game.Managers;
using XIVDeck.FFXIVPlugin.RpcServer.Helpers;
using XIVDeck.FFXIVPlugin.Contract;

namespace XIVDeck.FFXIVPlugin.RpcServer.Services;

[RpcService("Volume")]
public class VolumeService : IDisposable {
    private readonly VolumeManager _volumeManager;

    public event EventHandler<VolumeState>? VolumeChanged;

    public VolumeService(VolumeManager volumeManager) {
        this._volumeManager = volumeManager;

        this._volumeManager.VolumeChanged += this.OnVolumeChanged;
    }

    public void Dispose() {
        this._volumeManager.VolumeChanged -= this.OnVolumeChanged;

        GC.SuppressFinalize(this);
    }

    public GetAllChannelsResponse GetAllChannels() {
        Dictionary<SoundChannel, VolumeState> reply = new();

        foreach (var channel in Enum.GetValues<SoundChannel>()) {
            reply[channel] = this.GetChannel(channel);
        }

        return new GetAllChannelsResponse(reply);
    }

    public VolumeState GetChannel(SoundChannel channel) {
        return this._volumeManager.GetState(channel);
    }

    // Changing the level implies the user wants to hear it, so unmute unless told otherwise.
    public VolumeState SetChannel(SoundChannel channel, int? volume = null, bool? muted = null) {
        if (volume != null) {
            this._volumeManager.EnqueueVolumeChange(channel, (uint)Math.Clamp(volume.Value, 0, 100));
            muted ??= false;
        }

        if (muted != null && muted != this._volumeManager.IsMuted(channel)) {
            this._volumeManager.EnqueueMute(channel, muted.Value);
        }

        return this.GetChannel(channel);
    }

    public VolumeState DeltaChannel(SoundChannel channel, int delta) {
        return this.SetChannel(channel, (int)this._volumeManager.GetVolume(channel) + delta);
    }

    // Server-side, so rapid presses can't race a client-side read-then-write.
    public VolumeState ToggleMuteChannel(SoundChannel channel) {
        return this.SetChannel(channel, muted: !this._volumeManager.IsMuted(channel));
    }

    private void OnVolumeChanged(object? sender, VolumeState state) {
        this.VolumeChanged?.Invoke(this, state);
    }
}

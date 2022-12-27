using System;
using Dalamud.Logging;
using Newtonsoft.Json;
using XIVDeck.FFXIVPlugin.Game.Data;
using XIVDeck.FFXIVPlugin.Game.Watchers;

namespace XIVDeck.FFXIVPlugin.Server.Types; 

[Serializable]
public class SerializableVolumeMessage {
    private static VolumeWatcher _volumeWatcher = XIVDeckPlugin.Instance.VolumeWatcher;
    
    [JsonProperty("volume")]
    public int? Volume { get; set; }
    
    [JsonProperty("delta", NullValueHandling = NullValueHandling.Ignore)]
    public int? Delta { get; set; }
    
    [JsonProperty("muted")]
    public bool? Muted { get; set; }

    public SerializableVolumeMessage() { }
    
    public SerializableVolumeMessage(SoundChannel channel) {
        this.Volume = (int) _volumeWatcher.GetVolume(channel);
        this.Muted = _volumeWatcher.IsMuted(channel);
    }

    public void ApplyToChannel(SoundChannel channel) {
        var volumeToSet = this.Volume ?? (int) _volumeWatcher.GetVolume(channel);

        if (this.Muted != null) {
            _volumeWatcher.EnqueueMute(channel, this.Muted.Value);
            return;
        }

        if (this.Delta != null) {
            volumeToSet += this.Delta.Value;
        }

        if (volumeToSet < 0) volumeToSet = 0;
        if (volumeToSet > 100) volumeToSet = 100;

        if (_volumeWatcher.IsMuted(channel)) {
            _volumeWatcher.EnqueueMute(channel, false);
        }

        _volumeWatcher.EnqueueVolumeChange(channel, (uint) volumeToSet);
    }
}
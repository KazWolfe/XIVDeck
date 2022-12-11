using System;
using Newtonsoft.Json;
using XIVDeck.FFXIVPlugin.Game;
using XIVDeck.FFXIVPlugin.Game.Data;
using XIVDeck.FFXIVPlugin.Game.Managers;

namespace XIVDeck.FFXIVPlugin.Server.Types; 

[Serializable]
public class SerializableVolumeMessage {
    [JsonProperty("volume")]
    public int? Volume { get; set; }
    
    [JsonProperty("delta", NullValueHandling = NullValueHandling.Ignore)]
    public int? Delta { get; set; }
    
    [JsonProperty("muted")]
    public bool? Muted { get; set; }

    public SerializableVolumeMessage() { }
    
    public SerializableVolumeMessage(SoundChannel channel) {
        this.Volume = (int) VolumeManager.GetVolume(channel);
        this.Muted = VolumeManager.IsMuted(channel);
    }

    public void ApplyToChannel(SoundChannel channel) {
        var volumeToSet = this.Volume ?? (int) VolumeManager.GetVolume(channel);

        if (this.Muted != null) {
            VolumeManager.SetMuted(channel, this.Muted.Value);
            return;
        }

        if (this.Delta != null) {
            volumeToSet += this.Delta.Value;
        }

        if (volumeToSet < 0) volumeToSet = 0;
        if (volumeToSet > 100) volumeToSet = 100;

        if (VolumeManager.IsMuted(channel)) {
            VolumeManager.SetMuted(channel, false);
        }

        VolumeManager.SetVolume(channel, (uint) volumeToSet);
    }
}
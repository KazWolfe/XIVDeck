using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using XIVDeck.FFXIVPlugin.Game.Data;
using XIVDeck.FFXIVPlugin.Server.Types;

namespace XIVDeck.FFXIVPlugin.Server.Messages.Outbound; 

public class WSVolumeUpdateMessage : BaseOutboundMessage {
    private const string MessageName = "volumeUpdate";

    [JsonProperty("channel")] 
    [JsonConverter(typeof(StringEnumConverter))]
    public SoundChannel Channel { get; set; }
    [JsonProperty("data")] public SerializableVolumeMessage Data { get; set; }

    public WSVolumeUpdateMessage(SoundChannel channel, uint volume, bool muted) : base(MessageName) {
        this.Channel = channel;
        this.Data = new SerializableVolumeMessage {
            Volume = (int) volume,
            Muted = muted
        };
    }
}
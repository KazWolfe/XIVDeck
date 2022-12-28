using System.Threading.Tasks;
using Dalamud.Logging;
using EmbedIO.WebSockets;
using Newtonsoft.Json;
using XIVDeck.FFXIVPlugin.Game;
using XIVDeck.FFXIVPlugin.Game.Data;
using XIVDeck.FFXIVPlugin.Server.Helpers;
using XIVDeck.FFXIVPlugin.Server.Types;

namespace XIVDeck.FFXIVPlugin.Server.Messages.Inbound; 

[WSOpcode("setVolume")]
public class WSSetVolumeOpcode : BaseInboundMessage {
    [JsonProperty("channel")] public SoundChannel Channel;

    [JsonProperty("data")] public SerializableVolumeMessage? Data;
    
    public override Task Process(IWebSocketContext context) {
        this.Data?.ApplyToChannel(this.Channel);
        
        return Task.CompletedTask;
    }
}
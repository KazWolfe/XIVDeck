using System.Threading.Tasks;
using EmbedIO.WebSockets;
using Newtonsoft.Json;
using XIVDeck.FFXIVPlugin.Server.Helpers;
using XIVDeck.FFXIVPlugin.Server.Messages.Outbound;
using XIVDeck.FFXIVPlugin.Utils;

namespace XIVDeck.FFXIVPlugin.Server.Messages.Inbound;

[WSOpcode("ping")]
public class WSPingOpcode : BaseInboundMessage {
    [JsonProperty("value")]
    public string? Value;
    
    public override async Task Process(IWebSocketContext context) {
        await context.SendMessage(new WSPongMessage(this.Value));
    }
}
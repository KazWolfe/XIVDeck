using System.Threading.Tasks;
using EmbedIO.WebSockets;
using Newtonsoft.Json;

namespace XIVDeck.FFXIVPlugin.Server.Messages;

public class BaseInboundMessage {
    public string Opcode { get; set; } = default!;

    public BaseInboundMessage(string opcode) {
        this.Opcode = opcode;
    }

    public BaseInboundMessage() { }

    public virtual Task Process(IWebSocketContext context) {
        return Task.FromResult(true);
    }
}

public class BaseOutboundMessage {
    [JsonProperty("messageType")] public string MessageType { get; set; }
 
    public BaseOutboundMessage(string messageType) {
        this.MessageType = messageType;
    }
}
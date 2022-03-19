using System.Collections.Generic;
using NetCoreServer;
using Newtonsoft.Json;

namespace XIVDeck.FFXIVPlugin.Server.Messages.Inbound {
    public class WSEchoInboundMessage : BaseInboundMessage {
        public string Data { get; set; } = default!;

        public override WSEchoOutboundMessage Process(WsSession session) {
            return new WSEchoOutboundMessage(this.Data);
        }

        public WSEchoInboundMessage() : base("echo") { }
    }

    public class WSEchoOutboundMessage : BaseOutboundMessage {
        public string Data;

        public WSEchoOutboundMessage(string data) : base("echoReply") {
            this.Data = data;
        }
    }
}
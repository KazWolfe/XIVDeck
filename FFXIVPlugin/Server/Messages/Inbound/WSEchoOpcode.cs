
namespace XIVDeck.FFXIVPlugin.Server.Messages.Inbound {
    public class WSEchoInboundMessage : BaseInboundMessage {
        public string Data { get; set; } = default!;

        public override void Process(XIVDeckRoute session) {
            session.SendMessage(new WSEchoOutboundMessage(this.Data));
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
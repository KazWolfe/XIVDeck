using System.Collections.Generic;
using NetCoreServer;
using Newtonsoft.Json;

namespace XIVDeck.FFXIVPlugin.Server.Messages.Inbound {
    public class WSEchoInboundMessage : BaseInboundMessage {
        public string Data { get; set; } = default!;

        public override void Process(WsSession session) {
            var reply = new Dictionary<string, string> {
                ["data"] = this.Data
            };

            session.SendTextAsync(JsonConvert.SerializeObject(reply));
        }

        public WSEchoInboundMessage() : base("echo") { }
    }
}
using System;
using System.Collections.Generic;
using NetCoreServer;
using Newtonsoft.Json;

namespace FFXIVPlugin.Server.Messages.Inbound {
    public class WSEchoInboundMessage : BaseInboundMessage {
        public string Data { get; set; }

        public override void Process(WsSession session) {
            var reply = new Dictionary<string, string> {
                ["data"] = Data
            };

            session.SendTextAsync(JsonConvert.SerializeObject(reply));
        }

        public WSEchoInboundMessage() : base("echo") { }
    }
}
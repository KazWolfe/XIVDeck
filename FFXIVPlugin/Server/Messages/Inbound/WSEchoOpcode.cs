using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using WebSocketSharp;

namespace FFXIVPlugin.Server.Messages.Inbound {
    public class WSEchoInboundMessage : BaseInboundMessage {
        public string Data { get; set; }

        public override void Process(WebSocket socket) {
            var reply = new Dictionary<string, string>();
            reply["data"] = Data;

            socket.Send(JsonConvert.SerializeObject(reply));
        }

        public WSEchoInboundMessage() : base("echo") { }
    }
}
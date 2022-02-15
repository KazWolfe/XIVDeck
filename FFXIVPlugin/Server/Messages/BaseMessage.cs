using System;
using NetCoreServer;
using Newtonsoft.Json;

namespace FFXIVPlugin.Server.Messages.Inbound {
    public class BaseInboundMessage {
        public string Opcode { get; set; }
        public int Nonce { get; }

        public BaseInboundMessage(string opcode) {
            this.Opcode = opcode;
        }

        public virtual void Process(WsSession session) { }
    }

    public class BaseOutboundMessage {
        [JsonProperty("messageType")]
        public string MessageType { get; set; }
        
        [JsonProperty("nonce")]
        public int Nonce { get; set; }

        public BaseOutboundMessage(String messageType, int nonce) {
            this.MessageType = messageType;
            this.Nonce = nonce;
        }

        public BaseOutboundMessage(String messageType) {
            var rng = new Random();

            this.MessageType = messageType;
            this.Nonce = rng.Next();
        }
    }
}
using System;
using System.Diagnostics.CodeAnalysis;
using NetCoreServer;
using Newtonsoft.Json;

namespace FFXIVPlugin.Server.Messages {
    
    [SuppressMessage("ReSharper", "UnassignedGetOnlyAutoProperty", Justification = "JSON serializer will initialize these fields")]
    public class BaseInboundMessage {
        public string Opcode { get; set; }
        public string SDContext { get; set; }
        public int Nonce { get; }

        public BaseInboundMessage(string opcode, string sdContext = null) {
            this.Opcode = opcode;
            this.SDContext = sdContext;
        }

        public virtual void Process(WsSession session) { }
    }

    public class BaseOutboundMessage {
        [JsonProperty("messageType")] public string MessageType { get; set; }
        [JsonProperty("nonce")] public int Nonce { get; set; }

        public BaseOutboundMessage(string messageType, int nonce) {
            this.MessageType = messageType;
            this.Nonce = nonce;
        }

        public BaseOutboundMessage(string messageType) {
            var rng = new Random();

            this.MessageType = messageType;
            this.Nonce = rng.Next();
        }
    }
}
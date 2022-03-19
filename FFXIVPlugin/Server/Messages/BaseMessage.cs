using System;
using System.Diagnostics.CodeAnalysis;
using NetCoreServer;
using Newtonsoft.Json;

namespace XIVDeck.FFXIVPlugin.Server.Messages {
    
    [SuppressMessage("ReSharper", "UnassignedGetOnlyAutoProperty", Justification = "JSON serializer will initialize these fields")]
    public class BaseInboundMessage {
        public string Opcode { get; set; }
        public string? SDContext { get; set; }
        public dynamic? Context { get; set; } // context will just be parroted

        public BaseInboundMessage(string opcode, dynamic? context = null) {
            this.Opcode = opcode;
            this.Context = context;
        }

        public virtual BaseOutboundMessage? Process(WsSession session) {
            return null;
        }
    }

    public class BaseOutboundMessage {
        [JsonProperty("messageType")] public string MessageType { get; set; }
        
        [JsonProperty("context")] public dynamic? Context { get; set; }

        public BaseOutboundMessage(string messageType, dynamic? context = null) {
            this.MessageType = messageType;
            this.Context = context;
        }
    }
}
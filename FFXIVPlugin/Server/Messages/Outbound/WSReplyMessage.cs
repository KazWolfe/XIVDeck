using System;
using Newtonsoft.Json;

namespace XIVDeck.FFXIVPlugin.Server.Messages.Outbound {
    public class WSReplyMessage  : BaseOutboundMessage {
        private static string MESSAGE_NAME = "response";

        [JsonProperty("success")] public bool Success = true;
        [JsonProperty("exception")] public Exception? Exception;

        public WSReplyMessage() : base(MESSAGE_NAME) { }

        public WSReplyMessage(dynamic context, Exception? ex = null) : base(MESSAGE_NAME) {
            this.Context = context;

            if (ex != null) {
                this.Success = false;
                this.Exception = ex;
            }
        }
        
    }
}
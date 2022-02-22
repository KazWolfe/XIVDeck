using Newtonsoft.Json;

namespace FFXIVPlugin.Server.Messages.Outbound {
    public class WSShowSDAlert  : BaseOutboundMessage {
        private static string MESSAGE_NAME = "sdAlert";

        [JsonProperty("context")]
        public string Context { get; set; }

        public WSShowSDAlert() : base(MESSAGE_NAME) {
            
        }
    }
}
namespace FFXIVPlugin.Server.Messages.Inbound {
    public class WSClientReady  : BaseOutboundMessage {
        private static string MESSAGE_NAME = "gameReady";

        public string CharacterName;

        public WSClientReady() : base(MESSAGE_NAME) {
            
        }
    }
}
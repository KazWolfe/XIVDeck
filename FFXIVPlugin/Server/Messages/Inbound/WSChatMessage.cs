using NetCoreServer;

namespace FFXIVPlugin.Server.Messages.Inbound {
    public class WSChatMessage : BaseInboundMessage {
        public string Command { get; set; }

        public override void Process(WsSession session) {
            var plugin = XIVDeckPlugin.Instance;
            
            plugin.XivCommon.Functions.Chat.SendMessage(Command);
        }

        public WSChatMessage() : base("command") { }
    }
}
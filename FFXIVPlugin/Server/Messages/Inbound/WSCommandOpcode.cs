using NetCoreServer;

namespace FFXIVPlugin.Server.Messages.Inbound {
    public class WSCommandInboundMessage : BaseInboundMessage {
        public string Command { get; set; }

        public override void Process(WsSession session) {
            var plugin = XIVDeckPlugin.Instance;
            
            plugin.XivCommon.Functions.Chat.SendMessage(Command);
        }

        public WSCommandInboundMessage() : base("command") { }
    }
}
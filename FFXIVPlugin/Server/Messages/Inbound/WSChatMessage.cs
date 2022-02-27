using FFXIVPlugin.Utils;
using NetCoreServer;

namespace FFXIVPlugin.Server.Messages.Inbound {
    public class WSChatMessage : BaseInboundMessage {
        public string Command { get; set; }

        public override void Process(WsSession session) {
            var plugin = XIVDeckPlugin.Instance;
            
            TickScheduler.Schedule(delegate {
                plugin.XivCommon.Functions.Chat.SendMessage(this.Command);
            });
        }

        public WSChatMessage() : base("command") { }
    }
}
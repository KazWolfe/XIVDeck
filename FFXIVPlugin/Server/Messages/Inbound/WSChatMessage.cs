using FFXIVPlugin.helpers;
using FFXIVPlugin.Utils;
using Lumina.Excel.GeneratedSheets;
using NetCoreServer;

namespace FFXIVPlugin.Server.Messages.Inbound {
    public class WSChatMessage : BaseInboundMessage {
        public string Command { get; set; }

        public override void Process(WsSession session) {
            var plugin = XIVDeckPlugin.Instance;
            
            // threading, we intentionally ignore the return
            new TickScheduler(delegate {
                plugin.XivCommon.Functions.Chat.SendMessage(Command);
            }, Injections.Framework);
        }

        public WSChatMessage() : base("command") { }
    }
}
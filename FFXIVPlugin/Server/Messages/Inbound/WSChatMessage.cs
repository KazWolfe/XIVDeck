using System;
using NetCoreServer;
using XIVDeck.FFXIVPlugin.Base;
using XIVDeck.FFXIVPlugin.Utils;

namespace XIVDeck.FFXIVPlugin.Server.Messages.Inbound {
    public class WSChatMessage : BaseInboundMessage {
        public string Command { get; set; }

        public override void Process(WsSession session) {
            var plugin = XIVDeckPlugin.Instance;
            
            if (!Injections.ClientState.IsLoggedIn)
                throw new InvalidOperationException("A player is not logged in to the game!");
            
            TickScheduler.Schedule(delegate {
                plugin.XivCommon.Functions.Chat.SendMessage(this.Command);
            });
        }

        public WSChatMessage() : base("command") { }
    }
}
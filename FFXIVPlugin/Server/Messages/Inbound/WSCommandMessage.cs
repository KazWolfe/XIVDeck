using System;
using Dalamud.Logging;
using NetCoreServer;
using XivCommon.Functions;
using XIVDeck.FFXIVPlugin.Base;
using XIVDeck.FFXIVPlugin.Utils;

namespace XIVDeck.FFXIVPlugin.Server.Messages.Inbound {
    public class WSCommandMessage : BaseInboundMessage {
        public string Command { get; set; }
        public bool SafeMode = true; // this can be overridden by the serializer *if you know what you're doing*.
                                     // instructions will not be provided. 

        public override void Process(WsSession session) {
            if (!Injections.ClientState.IsLoggedIn)
                throw new InvalidOperationException("A player is not logged in to the game!");

            // only allow use of commands here for safety purposes (validating chat is hard)
            if (!this.Command.StartsWith("/") && this.SafeMode)
                throw new ArgumentException("Commands must start with a slash.");
            
            TickScheduler.Schedule(delegate {
                ChatUtil.SendSanitizedChatMessage(this.Command, this.SafeMode);
            });
        }

        public WSCommandMessage() : base("command") { }
    }
}
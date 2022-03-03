using System;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using XIVDeck.FFXIVPlugin.Utils;
using NetCoreServer;
using Newtonsoft.Json;
using XIVDeck.FFXIVPlugin.ActionExecutor;
using XIVDeck.FFXIVPlugin.Base;

namespace XIVDeck.FFXIVPlugin.Server.Messages.Inbound {
    public class WSExecuteActionOpcode : BaseInboundMessage {
        [JsonRequired] public ExecutableAction Action { get; set; }
        
        /**
         * Options to pass to the execution strategy (if any)
         */
        public dynamic Options { get; set; }

        public override void Process(WsSession session) {
            HotbarSlotType actionType = this.Action.HotbarSlotType;
            
            if (!Injections.ClientState.IsLoggedIn)
                throw new InvalidOperationException("A player is not logged in to the game!");
            
            ActionDispatcher.GetStrategyForSlotType(actionType).Execute((uint) this.Action.ActionId, this.Options);
        }

        public WSExecuteActionOpcode() : base("execAction") { }
    }
}
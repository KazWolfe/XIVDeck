using System;
using System.Collections.Generic;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using FFXIVPlugin.ActionExecutor;
using FFXIVPlugin.helpers;
using FFXIVPlugin.Utils;
using NetCoreServer;
using Newtonsoft.Json;

namespace FFXIVPlugin.Server.Messages.Inbound {
    public class WSExecuteActionOpcode : BaseInboundMessage {
        [JsonRequired] public ExecutableAction Action { get; set; }
        
        /**
         * Options to pass to the execution strategy (if any)
         */
        public dynamic Options { get; set; }

        public override void Process(WsSession session) {
            HotbarSlotType actionType = Action.HotbarSlotType;
            ActionDispatcher.GetStrategyForSlotType(actionType).Execute((uint) Action.ActionId, Options);
        }

        public WSExecuteActionOpcode() : base("execAction") { }
    }
}
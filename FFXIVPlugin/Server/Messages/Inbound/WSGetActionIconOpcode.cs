using System;
using System.Collections.Generic;
using NetCoreServer;
using Newtonsoft.Json;
using XIVDeck.FFXIVPlugin.ActionExecutor;

namespace XIVDeck.FFXIVPlugin.Server.Messages.Inbound {
    public class WSGetActionIconOpcode : BaseInboundMessage { 
        [JsonRequired][JsonProperty("action")] public ExecutableAction Action = default!;

        public override WSActionIconMessage Process(WsSession session) {
            var plugin = XIVDeckPlugin.Instance;

            var iconId = ActionDispatcher.GetStrategyForSlotType(this.Action.HotbarSlotType).GetIconId((uint) this.Action.ActionId);
            var pngString = plugin.IconManager.GetIconAsPngString(iconId % 1000000, iconId >= 1000000);
            
            return new WSActionIconMessage(this.Action, iconId, pngString);
        }

        public WSGetActionIconOpcode() : base("getActionIcon") { }
    }
    
    public class WSActionIconMessage : BaseOutboundMessage {
        [JsonProperty("action")]
        public ExecutableAction Action { get; }
        [JsonProperty("iconId")]
        public int IconId { get; }
        [JsonProperty("iconData")]
        public string IconData { get; }

        public WSActionIconMessage(ExecutableAction action, int iconId, string iconData) : base("actionIcon") {
            this.Action = action;
            this.IconId = iconId;
            this.IconData = iconData;
        }
    }
}
using System;
using System.Collections.Generic;
using NetCoreServer;
using Newtonsoft.Json;
using XIVDeck.FFXIVPlugin.ActionExecutor;

namespace XIVDeck.FFXIVPlugin.Server.Messages.Inbound {
    public class WSGetActionIconOpcode : BaseInboundMessage { 
        [JsonRequired][JsonProperty("action")] public ExecutableAction Action = default!;

        public override void Process(WsSession session) {
            var plugin = XIVDeckPlugin.Instance;

            var iconId = ActionDispatcher.GetStrategyForSlotType(this.Action.HotbarSlotType).GetIconId((uint) this.Action.ActionId);
            String pngString = plugin.IconManager.GetIconAsPngString(iconId % 1000000, iconId >= 1000000);

            var reply = new Dictionary<string, dynamic> {
                ["messageType"] = "actionIcon",
                ["action"] = this.Action,
                ["iconId"] = iconId,
                ["iconData"] = pngString
            };

            session.SendTextAsync(JsonConvert.SerializeObject(reply));
        }

        public WSGetActionIconOpcode() : base("getActionIcon") { }

    }
}
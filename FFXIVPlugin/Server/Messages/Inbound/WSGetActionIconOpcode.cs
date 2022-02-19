using System;
using System.Collections.Generic;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using FFXIVPlugin.ActionExecutor;
using FFXIVPlugin.helpers;
using NetCoreServer;
using Newtonsoft.Json;

namespace FFXIVPlugin.Server.Messages.Inbound {
    public class WSGetActionIconOpcode : BaseInboundMessage {
        [JsonRequired] public ExecutableAction action;

        public override void Process(WsSession session) {
            var plugin = XIVDeckPlugin.Instance;

            var iconId = ActionDispatcher.GetStrategyForSlotType(action.HotbarSlotType).GetIconId((uint) action.ActionId);
            String pngString = plugin.IconManager.GetIconAsPngString(iconId % 1000000, iconId >= 1000000);

            var reply = new Dictionary<string, dynamic>();
            reply["messageType"] = "actionIcon";
            reply["action"] = action;
            reply["iconId"] = iconId;
            reply["iconData"] = pngString;

            session.SendTextAsync(JsonConvert.SerializeObject(reply));
        }
        
        public WSGetActionIconOpcode() : base("getActionIcon") { }

    }
}
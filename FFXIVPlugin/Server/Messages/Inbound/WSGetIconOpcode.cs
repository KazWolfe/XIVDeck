using System;
using System.Collections.Generic;
using NetCoreServer;
using Newtonsoft.Json;

namespace XIVDeck.FFXIVPlugin.Server.Messages.Inbound {
    public class WSGetIconOpcode : BaseInboundMessage {
        [JsonRequired][JsonProperty("iconId")] public int IconId { get; set; }

        public override void Process(WsSession session) {
            var plugin = XIVDeckPlugin.Instance;
            
            String pngString = plugin.IconManager.GetIconAsPngString(this.IconId % 1000000, this.IconId >= 1000000);
            
            var reply = new Dictionary<string, dynamic> {
                ["messageType"] = "icon",
                ["iconId"] = this.IconId,
                ["iconData"] = pngString
            };

            session.SendTextAsync(JsonConvert.SerializeObject(reply));
        }
        
        public WSGetIconOpcode() : base("getIcon") { }

    }
}
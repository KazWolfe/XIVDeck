using System;
using System.Collections.Generic;
using NetCoreServer;
using Newtonsoft.Json;
using XIVDeck.FFXIVPlugin.Server.Messages.Outbound;

namespace XIVDeck.FFXIVPlugin.Server.Messages.Inbound {
    public class WSGetIconOpcode : BaseInboundMessage {
        [JsonRequired][JsonProperty("iconId")] public int IconId { get; set; }

        public override WSIconMessage Process(WsSession session) {
            var plugin = XIVDeckPlugin.Instance;
            var pngString = plugin.IconManager.GetIconAsPngString(this.IconId % 1000000, this.IconId >= 1000000);

            return new WSIconMessage(this.IconId, pngString);
        }
        
        public WSGetIconOpcode() : base("getIcon") { }

    }
    
    public class WSIconMessage  : BaseOutboundMessage {
        [JsonProperty("iconId")] public int IconId;
        [JsonProperty("iconData")] public string IconData;
        
        public WSIconMessage(int iconId, string iconData) : base("icon") {
            this.IconId = iconId;
            this.IconData = iconData;
        }
        
    }
}
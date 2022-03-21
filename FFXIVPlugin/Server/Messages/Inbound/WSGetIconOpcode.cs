
using Newtonsoft.Json;

namespace XIVDeck.FFXIVPlugin.Server.Messages.Inbound {
    public class WSGetIconOpcode : BaseInboundMessage {
        [JsonRequired][JsonProperty("iconId")] public int IconId { get; set; }

        public override void Process(XIVDeckRoute session) {
            var plugin = XIVDeckPlugin.Instance;
            var pngString = plugin.IconManager.GetIconAsPngString(this.IconId % 1000000, this.IconId >= 1000000);

            session.SendMessage(new WSIconMessage(this.IconId, pngString));
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
using System;
using System.Collections.Generic;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using NetCoreServer;
using Newtonsoft.Json;

namespace XIVDeck.FFXIVPlugin.Server.Messages.Inbound {
    public class WSGetHotbarSlotIconOpcode : BaseInboundMessage {
        [JsonRequired][JsonProperty("hotbarId")] public int HotbarId { get; set; }
        [JsonRequired][JsonProperty("slotId")] public int SlotId { get; set; }

        public override unsafe void Process(WsSession session) {
            var plugin = XIVDeckPlugin.Instance;

            var hotbarModule =
                Framework.Instance()->GetUiModule()->
                    GetRaptureHotbarModule();

            switch (this.HotbarId) {
                // Safety checks
                case < 0 or > 17:
                    throw new ArgumentException("Hotbar ID must be between 0 and 17");
                case < 10 when this.SlotId is < 0 or > 11:
                    // Hotbars 0-9 are normal hotbars
                    throw new ArgumentException("When hotbarID < 10, Slot ID must be between 0 and 11");
                case >= 10 when this.SlotId is < 0 or > 15:
                    // Hotbars 11-17 are cross hotbars
                    throw new ArgumentException("When Hotbar ID >= 10, Slot ID must be between 0 and 15");
            }

            var hotbarItem = hotbarModule->HotBar[this.HotbarId]->Slot[this.SlotId];
            var iconId = hotbarItem->Icon;
            
            String pngString = plugin.IconManager.GetIconAsPngString(iconId % 1000000, iconId >= 1000000);
            
            var reply = new Dictionary<string, dynamic> {
                ["messageType"] = "hotbarIcon",
                ["hotbarId"] = this.HotbarId,
                ["slotId"] = this.SlotId,
                ["iconId"] = iconId,
                ["iconData"] = pngString
            };

            session.SendTextAsync(JsonConvert.SerializeObject(reply));
        }
        
        public WSGetHotbarSlotIconOpcode() : base("getHotbarIcon") { }

    }
}
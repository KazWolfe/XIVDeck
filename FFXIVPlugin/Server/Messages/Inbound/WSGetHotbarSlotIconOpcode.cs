using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using WebSocketSharp;

namespace FFXIVPlugin.Server.Messages.Inbound {
    public class WSGetHotbarSlotIconOpcode : BaseInboundMessage {
        [JsonRequired]
        public int hotbarId { get; set; }
        [JsonRequired]
        public int slotId { get; set; }

        public unsafe override void Process(WebSocket socket) {
            var plugin = XIVDeckPlugin.Instance;

            var hotbarModule =
                FFXIVClientStructs.FFXIV.Client.System.Framework.Framework.Instance()->GetUiModule()->
                    GetRaptureHotbarModule();

            // Safety checks
            if (hotbarId is < 0 or > 17) throw new ArgumentException("Hotbar ID must be between 0 and 17");
            if (hotbarId < 10 && slotId is < 0 or > 11) {
                // Hotbars 0-9 are normal hotbars
                throw new ArgumentException("When hotbarID < 10, Slot ID must be between 0 and 11");
            }

            if (hotbarId >= 10 && slotId is < 0 or > 15) {
                // Hotbars 11-17 are cross hotbars
                throw new ArgumentException("When Hotbar ID >= 10, Slot ID must be between 0 and 15");
            }

            var hotbarItem = hotbarModule->HotBar[hotbarId]->Slot[slotId];
            var iconId = hotbarItem->Icon;
            
            String pngString = plugin.IconManager.GetIconAsPngString(iconId % 1000000, iconId >= 1000000);
            
            var reply = new Dictionary<string, dynamic>();
            reply["messageType"] = "hotbarIcon";
            reply["hotbarId"] = hotbarId;
            reply["slotId"] = slotId;
            reply["iconId"] = iconId;
            reply["iconData"] = pngString;

            socket.Send(JsonConvert.SerializeObject(reply));
        }
        
        public WSGetHotbarSlotIconOpcode() : base("getHotbarIcon") { }

    }
}
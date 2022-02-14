using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using WebSocketSharp;

namespace FFXIVPlugin.Server.Messages.Inbound {
    public class WSExecuteHotbarSlotOpcode : BaseInboundMessage {
        [JsonRequired] public int HotbarId { get; set; }
        
        [JsonRequired] public int SlotId { get; set; }

        public unsafe override void Process(WebSocket socket) {
            var plugin = XIVDeckPlugin.Instance;
            
            var hotbarModule =
                FFXIVClientStructs.FFXIV.Client.System.Framework.Framework.Instance()->GetUiModule()->
                    GetRaptureHotbarModule();
            
            // Safety checks
            if (HotbarId is < 0 or > 17) throw new ArgumentException("Hotbar ID must be between 0 and 17");
            if (HotbarId < 10 && SlotId is < 0 or > 11) {
                // Hotbars 0-9 are normal hotbars
                throw new ArgumentException("When hotbarID < 10, Slot ID must be between 0 and 11");
            }
            if (HotbarId >= 10 && SlotId is < 0 or > 15) {
                // Hotbars 11-17 are cross hotbars
                throw new ArgumentException("When Hotbar ID >= 10, Slot ID must be between 0 and 15");
            }
                
            var hotbarItem = hotbarModule->HotBar[HotbarId]->Slot[SlotId];
            plugin.SigHelper.ExecuteHotbarSlot(hotbarItem);
        }

        public WSExecuteHotbarSlotOpcode() : base("execHotbar") { }
    }
}
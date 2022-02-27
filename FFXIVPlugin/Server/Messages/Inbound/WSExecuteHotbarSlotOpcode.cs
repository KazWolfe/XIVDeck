using System;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using FFXIVPlugin.Utils;
using NetCoreServer;
using Newtonsoft.Json;

namespace FFXIVPlugin.Server.Messages.Inbound {
    public class WSExecuteHotbarSlotOpcode : BaseInboundMessage {
        [JsonRequired] public int HotbarId { get; set; }
        [JsonRequired] public int SlotId { get; set; }

        public override unsafe void Process(WsSession session) {
            var plugin = XIVDeckPlugin.Instance;
            
            var hotbarModule = Framework.Instance()->GetUiModule()->GetRaptureHotbarModule();

            switch (this.HotbarId) {
                // Safety checks
                case < 0 or > 17:
                    throw new ArgumentException("Hotbar ID must be between 0 and 17");
                case < 10 when this.SlotId is < 0 or > 11: // normal hotbars
                    throw new ArgumentException("When hotbarID < 10, Slot ID must be between 0 and 11");
                case >= 10 when this.SlotId is < 0 or > 15: // cross hotbars
                    throw new ArgumentException("When Hotbar ID >= 10, Slot ID must be between 0 and 15");
            }

            // Trigger the hotbar event on the next Framework tick, and also in the Framework (game main) thread.
            // For whatever reason, the game *really* doesn't like when a user casts a Weaponskill or Ability from a
            // non-game thread (as would be the case for API calls). Why this works normally for Spells and other
            // actions will forever be a mystery. 
            TickScheduler.Schedule(delegate { 
                var hotbarItem = hotbarModule->HotBar[this.HotbarId]->Slot[this.SlotId];
                plugin.SigHelper.ExecuteHotbarSlot(hotbarItem);
            });
        }

        public WSExecuteHotbarSlotOpcode() : base("execHotbar") { }
    }
}
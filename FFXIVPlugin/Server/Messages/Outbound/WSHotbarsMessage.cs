using System.Collections.Generic;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace XIVDeck.FFXIVPlugin.Server.Messages.Outbound {
    public class SerialiazableHotbarSlot {
        public uint CommandId;
        
        [JsonConverter(typeof(StringEnumConverter))] public HotbarSlotType CommandType;
        
        public int IconId;
        public string PngData;
        
        // stops newtonsoft from complaining
        public SerialiazableHotbarSlot() { }

        public unsafe SerialiazableHotbarSlot(HotBarSlot* slot) {
            this.CommandId = slot->CommandId;
            this.CommandType = slot->CommandType;

            this.IconId = slot->Icon;
            // this.PngData = XIVDeckPlugin.Instance.IconManager.GetIconAsPngString(IconId % 1000000, IconId >= 1000000);
        }
    }
    public class WSHotbarUpdateMessage : BaseOutboundMessage {
        private static string MESSAGE_NAME = "hotbarUpdate";

        [JsonProperty("hotbarData")] public List<List<SerialiazableHotbarSlot>> HotbarData { get; set; } = new();

        public unsafe WSHotbarUpdateMessage(HotBars hotBars) : base(MESSAGE_NAME) {
            for (int i = 0; i < 17; i++) {
                HotbarData.Add(new List<SerialiazableHotbarSlot>());

                for (int j = 0; j < 15; j++) {
                    HotbarData[i].Add(new SerialiazableHotbarSlot(hotBars[i]->Slot[j]));
                }
            }
        }
    }
}
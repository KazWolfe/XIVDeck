using System.Collections.Generic;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace XIVDeck.FFXIVPlugin.Server.Messages.Outbound {
    public class SerializableHotbarSlot {
        public uint CommandId;

        [JsonConverter(typeof(StringEnumConverter))]
        public HotbarSlotType CommandType;

        public int IconId;
        public string? PngData;
        
        public unsafe SerializableHotbarSlot(HotBarSlot* slot) {
            this.CommandId = slot->CommandId;
            this.CommandType = slot->CommandType;
            this.IconId = slot->Icon;
        }

        public SerializableHotbarSlot(HotBarSlot slot) {
            this.CommandId = slot.CommandId;
            this.CommandType = slot.CommandType;
            this.IconId = slot.Icon;
        }
    }
    public class WSHotbarUpdateMessage : BaseOutboundMessage {
        private static string MESSAGE_NAME = "hotbarUpdate";

        [JsonProperty("hotbarData")] public List<List<SerializableHotbarSlot>> HotbarData { get; set; } = new();

        public WSHotbarUpdateMessage(HotBarSlot[,] hotBars) : base(MESSAGE_NAME) {
            for (var i = 0; i < 17; i++) {
                this.HotbarData.Add(new List<SerializableHotbarSlot>());

                for (var j = 0; j < 15; j++) {
                    this.HotbarData[i].Add(new SerializableHotbarSlot(hotBars[i,j]));
                }
            }
        }
    }
}
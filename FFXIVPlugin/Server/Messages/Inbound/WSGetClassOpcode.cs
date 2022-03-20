using System;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Logging;
using Lumina.Excel;
using Lumina.Excel.GeneratedSheets;
using NetCoreServer;
using Newtonsoft.Json;
using XIVDeck.FFXIVPlugin.Base;
using XIVDeck.FFXIVPlugin.Game;
using XIVDeck.FFXIVPlugin.Server.Types;

namespace XIVDeck.FFXIVPlugin.Server.Messages.Inbound {
    public class WSGetClassOpcode : BaseInboundMessage {
        [JsonProperty("id")] public int ClassId;
        
        public override WSClassMessage Process(WsSession session) {
            return new WSClassMessage(this.ClassId);
        }

        public WSGetClassOpcode() : base("getClasses") { }
    }

    public class WSClassMessage : BaseOutboundMessage {
        [JsonProperty("class")] public SerializableGameClass GameClass { get; }
        
        public WSClassMessage(int classId) : base("gameClass") {
            this.GameClass = SerializableGameClass.GetCache()[classId];
        }
    }
}

using Newtonsoft.Json;
using XIVDeck.FFXIVPlugin.Server.Types;

namespace XIVDeck.FFXIVPlugin.Server.Messages.Inbound {
    public class WSGetClassOpcode : BaseInboundMessage {
        [JsonProperty("id")] public int ClassId;
        
        public override void Process(XIVDeckRoute session) {
            session.SendMessage(new WSClassMessage(this.ClassId));
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
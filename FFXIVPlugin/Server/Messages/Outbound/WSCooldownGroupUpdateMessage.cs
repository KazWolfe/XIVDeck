using System.Text.Json;
using Newtonsoft.Json;
using XIVDeck.FFXIVPlugin.Server.Types;

namespace XIVDeck.FFXIVPlugin.Server.Messages.Outbound; 

public class WSCooldownGroupUpdateMessage : BaseOutboundMessage {
    private const string MessageName = "cooldownUpdate";

    [JsonProperty("data")] public CooldownInfo Data;

    public WSCooldownGroupUpdateMessage(CooldownInfo data) : base(MessageName) {
        this.Data = data;
    }
}
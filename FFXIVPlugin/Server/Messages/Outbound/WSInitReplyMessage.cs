using Newtonsoft.Json;

namespace XIVDeck.FFXIVPlugin.Server.Messages.Outbound;

/**
 * A WebSocket message to disclose a generic update of state, generally for actions or some other case where it
 * would be a good idea for the Stream Deck plugin to refresh (a subset of) information.
 */
public class WSInitReplyMessage : BaseOutboundMessage {
    private static string MESSAGE_NAME = "initReply";
    
    [JsonProperty("version")] public string Version { get; set; }
    [JsonProperty("apiKey")] public string? ApiKey { get; set; }

    public WSInitReplyMessage(string version, string? apiKey = null) : base(MESSAGE_NAME) {
        this.Version = version;
        this.ApiKey = apiKey;
    }
}

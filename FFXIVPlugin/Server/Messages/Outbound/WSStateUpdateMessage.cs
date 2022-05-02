using Newtonsoft.Json;

namespace XIVDeck.FFXIVPlugin.Server.Messages.Outbound; 

/**
 * A WebSocket message to disclose a generic update of state, generally for actions or some other case where it
 * would be a good idea for the Stream Deck plugin to refresh (a subset of) information.
 */
public class WSStateUpdateMessage : BaseOutboundMessage {
    private static string MESSAGE_NAME = "stateUpdate";
        
    [JsonProperty("type")] public string StateType { get; set; }
    [JsonProperty("params")] public string? Parameters { get; set; }

    public WSStateUpdateMessage(string stateType, string parameters) : base(MESSAGE_NAME) {
        this.StateType = stateType;
        this.Parameters = parameters;
    }

    public WSStateUpdateMessage(string stateType) : base(MESSAGE_NAME) {
        this.StateType = stateType;
        this.Parameters = null;
    }
}
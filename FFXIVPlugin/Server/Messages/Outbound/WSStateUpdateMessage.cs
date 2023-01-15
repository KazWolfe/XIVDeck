using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace XIVDeck.FFXIVPlugin.Server.Messages.Outbound; 

/**
 * A WebSocket message to disclose a generic update of state, generally for actions or some other case where it
 * would be a good idea for the Stream Deck plugin to refresh (a subset of) information.
 */
public class WSStateUpdateMessage : BaseOutboundMessage {
    private const string MessageName = "stateUpdate";

    [JsonProperty("type")] public string StateType { get; set; }
    
    public WSStateUpdateMessage(string stateType) : base(MessageName) {
        this.StateType = stateType;
    }
}

public class WSStateUpdateMessage<T> : WSStateUpdateMessage {
    [JsonProperty("params")] public T? Parameters { get; set; }
    
    public WSStateUpdateMessage(string stateType, T parameters) : base(stateType) {
        this.Parameters = parameters;
    }
}
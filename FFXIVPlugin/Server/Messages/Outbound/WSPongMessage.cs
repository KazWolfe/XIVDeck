using System;
using Newtonsoft.Json;

namespace XIVDeck.FFXIVPlugin.Server.Messages.Outbound;

/**
 * A WebSocket message to disclose a generic update of state, generally for actions or some other case where it
 * would be a good idea for the Stream Deck plugin to refresh (a subset of) information.
 */
public class WSPongMessage : BaseOutboundMessage {
    private const string MessageName = "pong";

    [JsonProperty("value",  NullValueHandling = NullValueHandling.Ignore)] public string? Value { get; set; }
    [JsonProperty("timestamp")] public double Timestamp { get; set; }

    public WSPongMessage(string? value = null) : base(MessageName) {
        this.Value = value;
        this.Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() / 1000.0;
    }
}

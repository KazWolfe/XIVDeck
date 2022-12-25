using Newtonsoft.Json;

namespace XIVDeck.FFXIVPlugin.ActionExecutor.Payloads; 

public record EmotePayload : ActionPayload {

    /// <summary>
    /// Determine whether to send a log message or not.
    ///
    /// null: Use game defaults
    /// true: Always send log message
    /// false: Never send log message
    /// </summary>
    [JsonProperty("sendLogMessage")] public bool? SendLogMessage = null;
}
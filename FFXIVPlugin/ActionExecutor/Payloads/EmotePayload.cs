using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace XIVDeck.FFXIVPlugin.ActionExecutor.Payloads;

public enum EmoteLogMode {
    Default,
    Always,
    Never
}

public record EmotePayload : ActionPayload {

    /// <summary>
    /// Determine whether to send a log message or not.
    ///
    /// null: Use game defaults
    /// true: Always send log message
    /// false: Never send log message
    /// </summary>
    [JsonProperty("logMode")]
    [JsonConverter(typeof(StringEnumConverter), typeof(CamelCaseNamingStrategy))]
    public EmoteLogMode LogMode = EmoteLogMode.Default;
}
using Newtonsoft.Json;

namespace XIVDeck.FFXIVPlugin.ActionExecutor.Payloads; 

public record GearsetPayload : ActionPayload {
    [JsonProperty("glamourPlateId")] public uint? GlamourPlateId;
}
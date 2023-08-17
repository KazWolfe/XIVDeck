using System;
using Newtonsoft.Json;

namespace XIVDeck.FFXIVPlugin.Server.Types; 

[Serializable]
public class CooldownInfo {
    [JsonProperty("groupId")] public int CooldownGroupId { get; set; }
    [JsonProperty("active")] public bool CooldownActive { get; set; }
    [JsonProperty("lastActionId")] public uint LastActionId { get; set; }

    [JsonProperty("elapsedTime")] public float ElapsedChargeTime { get; set; }
    [JsonProperty("totalTime")] public float TotalChargeTime { get; set; }
    [JsonProperty("recastTime")] public float AdjustedRecastTime { get; set; }
    
    [JsonProperty("maxCharges")] public uint? MaxCharges { get; set; }
}
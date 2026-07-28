using PolyType;

namespace XIVDeck.FFXIVPlugin.ActionExecutor.Payloads;

[GenerateShape]
public partial record GearsetPayload(uint? GlamourPlateId) : ActionPayload;

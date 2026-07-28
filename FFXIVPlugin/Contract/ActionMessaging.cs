using System.Collections.Generic;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using PolyType;

namespace XIVDeck.FFXIVPlugin.Contract;

[GenerateShape]
public partial record GetActionsByTypeResponse(List<ActionEntry> Actions);

[GenerateShape]
public partial record GetActionsResponse(Dictionary<string, List<ActionEntry>> Actions);

[GenerateShape]
public partial record ActionTypeUpdateBatch(List<ActionTypeUpdateNotification> Updates);

[GenerateShape]
public partial record ActionTypeUpdateNotification {
    public required RaptureHotbarModule.HotbarSlotType UpdatedType;

    public uint? ActionId;
}

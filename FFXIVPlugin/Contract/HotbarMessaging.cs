using System.Collections.Generic;
using PolyType;

namespace XIVDeck.FFXIVPlugin.Contract;

public record HotbarSlotRef(int HotbarId, int SlotId);

/// <summary>
/// Notify the client (by reference) that a slot has been updated.
/// </summary>
[GenerateShape]
public partial record HotbarChangeNotification(List<HotbarSlotRef>? Changes = null);

/// <summary>
/// Request from the SD plugin to watch the specified slots.
/// </summary>
[GenerateShape]
public partial record HotbarWatchRequest(List<HotbarSlotRef>? Slots = null);

using System;
using PolyType;
using static FFXIVClientStructs.FFXIV.Client.UI.Misc.RaptureHotbarModule;

namespace XIVDeck.FFXIVPlugin.Contract;

/// <summary>
/// A record of an action that can be executed, to be sent to the Stream Deck for display purposes.
/// </summary>
[Serializable]
[GenerateShape]
public partial class ActionEntry {
    public string? Name;

    public HotbarSlotType Type;
    public int Id;

    /// <summary>
    /// A key to use to sort individual actions. If omitted, sort by ID instead.
    /// </summary>
    public int? SortOrder;

    /// <summary>
    /// The category this action belongs in.
    /// </summary>
    public string? Category;
}

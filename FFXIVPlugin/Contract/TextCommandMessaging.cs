using System;
using PolyType;

namespace XIVDeck.FFXIVPlugin.Contract;

[Serializable]
[GenerateShape]
public partial class TextCommandRequest {
    public string? Command { get; set; }
}

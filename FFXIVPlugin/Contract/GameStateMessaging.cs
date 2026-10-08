using PolyType;

namespace XIVDeck.FFXIVPlugin.Contract;

[GenerateShape]
public partial record FocusState {
    public bool IsFocused;
    public long? LastFocusTime;
}

using System.Collections.Generic;
using PolyType;

namespace XIVDeck.FFXIVPlugin.Contract;

[GenerateShape]
public partial record GetAllChannelsResponse(Dictionary<SoundChannel, VolumeState> Channels);

/// <summary>
/// A sound channel's current state.
/// </summary>
[GenerateShape]
public partial record VolumeState {
    public SoundChannel Channel;
    public int Volume;
    public bool Muted;
}

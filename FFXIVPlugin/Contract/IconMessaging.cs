using PolyType;

namespace XIVDeck.FFXIVPlugin.Contract;

[GenerateShape]
public partial record GetIconResponse(byte[] Png);

[GenerateShape]
public partial record ClearIconCacheMessage;

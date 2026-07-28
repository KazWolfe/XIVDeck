using System.Collections.Generic;
using PolyType;

namespace XIVDeck.FFXIVPlugin.Contract;

[GenerateShape]
public partial record GetClassesResponse(List<SerializableGameClass> Classes);

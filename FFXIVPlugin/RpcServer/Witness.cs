using Nerdbank.MessagePack;
using PolyType;

namespace XIVDeck.FFXIVPlugin.RpcServer;

// witness-ing requirements for Fancy Deserialization
[GenerateShapeFor<RawMessagePack>]
[GenerateShapeFor<RawMessagePack?>]
public partial class Witness;

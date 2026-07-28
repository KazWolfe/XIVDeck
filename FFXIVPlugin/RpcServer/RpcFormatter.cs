using Nerdbank.MessagePack;
using StreamJsonRpc;

namespace XIVDeck.FFXIVPlugin.RpcServer;

public static class RpcFormatter {
    public static readonly MessagePackSerializer UserDataSerializer = NerdbankMessagePackFormatter.DefaultSerializer with {
        PropertyNamingPolicy = MessagePackNamingPolicy.CamelCase,
        SerializeEnumValuesByName = true,
    };

    public static NerdbankMessagePackFormatter Create() => new() {
        TypeShapeProvider = Witness.GeneratedTypeShapeProvider,
        UserDataSerializer = UserDataSerializer,
    };
}

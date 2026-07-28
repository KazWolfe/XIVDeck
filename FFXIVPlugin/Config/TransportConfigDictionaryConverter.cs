using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace XIVDeck.FFXIVPlugin.Config;

public class TransportConfigDictionaryConverter : JsonConverter<Dictionary<TransportType, TransportConfig>> {
    public override void WriteJson(JsonWriter writer, Dictionary<TransportType, TransportConfig>? value,
        JsonSerializer serializer) {
        serializer.Serialize(writer, value, typeof(object));
    }

    public override Dictionary<TransportType, TransportConfig> ReadJson(JsonReader reader,
        Type objectType, Dictionary<TransportType, TransportConfig>? existingValue, bool hasExistingValue,
        JsonSerializer serializer) {
        var result = new Dictionary<TransportType, TransportConfig>();
        var jObject = JObject.Load(reader);

        foreach (var (key, jToken) in jObject) {
            if (jToken == null || !Enum.TryParse<TransportType>(key, out var transportType)) {
                continue;
            }

            var concreteType = transportType switch {
                TransportType.WebSocket => typeof(WebSocketTransportConfig),
                TransportType.UnixDomainSocket => typeof(UnixSocketTransportConfig),
                TransportType.NamedPipe => typeof(NamedPipeTransportConfig),
                _ => throw new ArgumentOutOfRangeException(nameof(transportType), transportType, @"Unknown transport type."),
            };

            if (jToken.ToObject(concreteType, serializer) is TransportConfig config) {
                result[transportType] = config;
            }
        }

        return result;
    }
}

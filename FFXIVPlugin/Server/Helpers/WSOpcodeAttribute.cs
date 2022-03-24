using System;
using System.Collections.Generic;
using System.Reflection;
using Dalamud.Logging;
using Newtonsoft.Json;
using XIVDeck.FFXIVPlugin.Server.Messages;

namespace XIVDeck.FFXIVPlugin.Server.Helpers;

[AttributeUsage(AttributeTargets.Class)]
public class WSOpcodeAttribute : Attribute {
    public readonly string Opcode;

    public WSOpcodeAttribute(string opcode) {
        this.Opcode = opcode;
    }
}

public static class WSOpcodeWiring {
    private static Dictionary<string, Type> Opcodes = new();

    public static void Autowire() {
        var assembly = Assembly.GetExecutingAssembly();

        foreach (var type in assembly.GetTypes()) {
            var opcodeAttribute = type.GetCustomAttribute<WSOpcodeAttribute>();

            if (opcodeAttribute != null) {
                Opcodes[opcodeAttribute.Opcode] = type;
                PluginLog.Debug($"Registered WebSocket opcode {opcodeAttribute.Opcode}");
            }
        }
    }

    public static BaseInboundMessage? GetInstance(string opcode, string jsonData) {
        if (!Opcodes.ContainsKey(opcode)) {
            throw new ArgumentOutOfRangeException(nameof(opcode), $"No message exists with opcode {opcode}");
        }

        return (BaseInboundMessage?) JsonConvert.DeserializeObject(jsonData, Opcodes[opcode]);
    }
}
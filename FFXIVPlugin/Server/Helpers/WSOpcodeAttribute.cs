using System;
using System.Collections.Generic;
using System.Reflection;

using Newtonsoft.Json;
using XIVDeck.FFXIVPlugin.Base;
using XIVDeck.FFXIVPlugin.Resources.Localization;
using XIVDeck.FFXIVPlugin.Server.Messages;

namespace XIVDeck.FFXIVPlugin.Server.Helpers;

[AttributeUsage(AttributeTargets.Class)]
public class WSOpcodeAttribute : Attribute {
    public readonly string Opcode;

    public WSOpcodeAttribute(string opcode) {
        this.Opcode = opcode;
    }
}

public class WSOpcodeWiring {
    private readonly Dictionary<string, Type> _opcodes = new();

    public void Autowire() {
        var assembly = Assembly.GetExecutingAssembly();

        foreach (var type in assembly.GetTypes()) {
            var opcodeAttribute = type.GetCustomAttribute<WSOpcodeAttribute>();

            if (opcodeAttribute != null) {
                this._opcodes[opcodeAttribute.Opcode] = type;
                Injections.PluginLog.Debug($"Registered WebSocket opcode {opcodeAttribute.Opcode}");
            }
        }
    }

    public BaseInboundMessage? GetInstance(string opcode, string jsonData) {
        if (!this._opcodes.ContainsKey(opcode)) {
            throw new ArgumentOutOfRangeException(nameof(opcode), 
                string.Format(UIStrings.WSOpcodeWiring_UnknownOpcodeError, opcode));
        }

        return (BaseInboundMessage?) JsonConvert.DeserializeObject(jsonData, this._opcodes[opcode]);
    }
}
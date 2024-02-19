using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using XIVDeck.FFXIVPlugin.Base;

namespace XIVDeck.FFXIVPlugin.UI;

public enum LinkCode {
    GetGithubReleaseLink
}

public interface IChatLinkHandler {
    public void Handle(uint opcode, SeString clickedString);
}


[AttributeUsage(AttributeTargets.Class)]
public class ChatLinkHandlerAttribute : Attribute {
    public readonly LinkCode Opcode;

    public ChatLinkHandlerAttribute(LinkCode opcode) {
        this.Opcode = opcode;
    }
}


/**
 * This is *absolutely totally overkill* in every way, shape, and form, but exists to serve a "purpose".
 *
 * If a new chat link is ever added, it can simply be added to the ChatLinkHandlers folder and tack on the above
 * attribute to be handled. Autowiring using this pattern just keeps me from having to register a bunch of different
 * types.
 */
public class ChatLinkWiring : IDisposable {
    private static readonly Dictionary<LinkCode, DalamudLinkPayload> Payloads = new();
    
    public static DalamudLinkPayload GetPayload(LinkCode linkCode) {
        if (!Payloads.ContainsKey(linkCode)) {
            throw new ArgumentException($"No handler is registered for Link Code {linkCode.ToString()}");
        }
        
        return Payloads[linkCode];
    }

    public ChatLinkWiring() {
        var assembly = Assembly.GetExecutingAssembly();

        foreach (var type in assembly.GetTypes()) {
            if (!type.GetInterfaces().Contains(typeof(IChatLinkHandler))) {
                continue;
            }
            
            var attr = type.GetCustomAttribute<ChatLinkHandlerAttribute>();
            if (attr == null) continue;
            
            var opcode = attr.Opcode;

            var handler = (IChatLinkHandler) Activator.CreateInstance(type)!;
            
            // Mitigates an issue where registering a chat link can sometimes throw an exception that it's already
            // been registered. Thanks for the tip, Kami!
            Injections.PluginInterface.RemoveChatLinkHandler((uint) opcode);
                
            Injections.PluginLog.Debug($"Registered chat link handler for opcode {attr.Opcode}: {handler.GetType()}");
            Payloads[opcode] = Injections.PluginInterface.AddChatLinkHandler((uint) opcode, handler.Handle);
        }
    }

    public void Dispose() {
        foreach (var (code, _) in Payloads) {
            Injections.PluginInterface.RemoveChatLinkHandler((uint) code);
            Payloads.Remove(code);
        }

        GC.SuppressFinalize(this);
    }
}
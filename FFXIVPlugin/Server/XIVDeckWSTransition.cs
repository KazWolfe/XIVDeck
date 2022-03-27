using System;
using System.Threading.Tasks;
using Dalamud.Logging;
using EmbedIO.WebSockets;
using Newtonsoft.Json;
using XIVDeck.FFXIVPlugin.Server.Helpers;
using XIVDeck.FFXIVPlugin.Server.Messages;
using XIVDeck.FFXIVPlugin.UI.Windows.Nags;

namespace XIVDeck.FFXIVPlugin.Server; 

public class XIVDeckWSTransition : WebSocketModule {
        
    public XIVDeckWSTransition(string urlPath) : base(urlPath, true) { }

    protected override async Task OnMessageReceivedAsync(IWebSocketContext context, byte[] buffer, IWebSocketReceiveResult result) {
        
        PluginLog.Warning($"SD plugin attempted to connect to old endpoint...");
        ForcedUpdateNag.Show();
        
        await context.WebSocket.CloseAsync(CloseStatusCode.PolicyViolation, 
            "XIVDeck SD Plugin outdated", context.CancellationToken);
    }
}
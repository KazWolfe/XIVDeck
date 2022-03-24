using System.Text;
using System.Threading.Tasks;
using EmbedIO.WebSockets;
using Newtonsoft.Json;
using XIVDeck.FFXIVPlugin.Server.Messages;

namespace XIVDeck.FFXIVPlugin.Utils; 

public static class WebUtils {
    public static async Task SendMessage(IWebSocketContext context, BaseOutboundMessage message) {
        var serializedData = JsonConvert.SerializeObject(message);
        var encoded = Encoding.UTF8.GetBytes(serializedData);
        
        await context.WebSocket.SendAsync(encoded, true, context.CancellationToken);
    }
}
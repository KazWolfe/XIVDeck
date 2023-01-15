using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dalamud.Logging;
using EmbedIO.WebSockets;
using Newtonsoft.Json;
using XIVDeck.FFXIVPlugin.Server.Helpers;
using XIVDeck.FFXIVPlugin.Server.Messages;

namespace XIVDeck.FFXIVPlugin.Server; 

public class XIVDeckWSServer : WebSocketModule {
    public static XIVDeckWSServer? Instance { get; private set; }
        
    public XIVDeckWSServer(string urlPath) : base(urlPath, true) {
        // this is a bit hacky, but *should* be alright as the server shouldn't actually ever create more than one.
        // if it does, the instance will be replaced and any notifiers will point to the new (proper) place. I hope.
        Instance = this;
        WSOpcodeWiring.Autowire();
    }

    protected override async Task OnMessageReceivedAsync(IWebSocketContext context, byte[] buffer, IWebSocketReceiveResult result) {
        // crash protection - prevents WS from taking down all of FFXIV if a message fails to decode
            
        try {
            await this._onMessage(context, buffer);
        } catch (Exception ex) {
            PluginLog.Error(ex, "Got an exception on websocket process");
        }
    }

    private async Task _onMessage(IWebSocketContext context, byte[] buffer) {
        var rawData = this.Encoding.GetString(buffer);
        var message = JsonConvert.DeserializeObject<BaseInboundMessage>(rawData);

        if (message == null) {
            PluginLog.Warning($"WebSocket message failed to deserialize to base: {rawData}");
            return;
        }

        var instance = WSOpcodeWiring.GetInstance(message.Opcode, rawData);

        if (instance == null) {
            PluginLog.Warning($"WebSocket message failed to deserialize to instance: {rawData}");
            return ;
        }

        await instance.Process(context);
    }

    internal IReadOnlyList<IWebSocketContext> Connections => this.ActiveContexts;

    public new void Dispose() {
        Instance = null;
        base.Dispose();
    }

    public void BroadcastMessage(BaseOutboundMessage message) {
        Task.Run(async () => {
            var serializedData = JsonConvert.SerializeObject(message);
            await this.BroadcastAsync(serializedData);
        });
    }
}
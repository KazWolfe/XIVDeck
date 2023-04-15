using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Dalamud.Logging;
using EmbedIO.WebSockets;
using Newtonsoft.Json;
using XIVDeck.FFXIVPlugin.Server.Helpers;
using XIVDeck.FFXIVPlugin.Server.Messages;
using XIVDeck.FFXIVPlugin.Utils;

namespace XIVDeck.FFXIVPlugin.Server; 

public class XIVDeckWSServer : WebSocketModule {
    private readonly SemaphoreSlim _sendLock = new(1, 1);

    private readonly WSOpcodeWiring _wsModules = new();
        
    public XIVDeckWSServer(string urlPath) : base(urlPath, true) {
        this._wsModules.Autowire();
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

        var instance = this._wsModules.GetInstance(message.Opcode, rawData);

        if (instance == null) {
            PluginLog.Warning($"WebSocket message failed to deserialize to instance: {rawData}");
            return ;
        }

        await instance.Process(context);
    }

    internal IReadOnlyList<IWebSocketContext> Connections => this.ActiveContexts;

    protected override void Dispose(bool disposing) {
        base.Dispose(disposing);
        
        PluginLog.Debug("WS server is going away!");
        this._sendLock.Dispose();
    }

    public void BroadcastMessage(BaseOutboundMessage message) {
        Task.Run(async () => {
            using (await this._sendLock.UseWaitAsync()) {
                var serializedData = JsonConvert.SerializeObject(message);
                await this.BroadcastAsync(serializedData);
            }
        });
    }
}
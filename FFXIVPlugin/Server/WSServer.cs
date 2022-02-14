using Dalamud.Logging;
using FFXIVPlugin.helpers;
using FFXIVPlugin.Server.Messages.Inbound;
using Newtonsoft.Json;
using WebSocketSharp;
using WebSocketSharp.Server;
using XivCommon;

namespace FFXIVPlugin.Server {
    public class WSServer {
        public string url;
        public WebSocketServer server;

        public WSServer(int port) {
            this.url = $"ws://localhost:{port}";
            this.server = new WebSocketServer(this.url);
            
            server.AddWebSocketService<SocketRoute>("/xivdeck");
            server.Start();
            PluginLog.Information($"WebSocket server started at {url}");
        }

        public void Dispose() {
            server.Stop();
        }
    }

    public class SocketRoute : WebSocketBehavior {
        protected override void OnMessage(MessageEventArgs e) {
            PluginLog.Information($"Got WS message - {e.Data}");

            var message = JsonConvert.DeserializeObject<BaseInboundMessage>(e.Data);

            switch (message.Opcode) {
                case "command":
                    message = JsonConvert.DeserializeObject<WSCommandInboundMessage>(e.Data);
                    break;
                case "echo":
                    message = JsonConvert.DeserializeObject<WSEchoInboundMessage>(e.Data);
                    break;
                case "execHotbar":
                    message = JsonConvert.DeserializeObject<WSExecuteHotbarSlotOpcode>(e.Data);
                    break;
                case "getHotbarIcon":
                    message = JsonConvert.DeserializeObject<WSGetHotbarSlotIconOpcode>(e.Data);
                    break;
                case "init":
                    message = JsonConvert.DeserializeObject<WSInitMessage>(e.Data);
                    break;
            }

            message.Process(this.Context.WebSocket);
        }

        protected override void OnOpen() {
            if (!Context.IsLocal) {
                this.Send("For security purposes, non-local connections are blocked.");
                this.Context.WebSocket.Close();
            }

            /* if (Sessions.Count > 1) {
                this.Send("Only a single session can be opened at once.");
                this.Context.WebSocket.Close();
            } */
        }

        protected override void OnError(WebSocketSharp.ErrorEventArgs e) {
            Injections.Chat.PrintError($"[XIVDeck] An error occured while processing a websocket message: {e.Exception.GetType()}: {e.Exception.Message}");
            
            // throw the exception upstream anyways so it gets into our log
            throw e.Exception;
        }
    }
}
using System;
using System.IO;
using System.Net;
using System.Text;
using Dalamud.Logging;
using NetCoreServer;
using Newtonsoft.Json;
using XIVDeck.FFXIVPlugin.Base;
using XIVDeck.FFXIVPlugin.Server.Messages;
using XIVDeck.FFXIVPlugin.Server.Messages.Inbound;
using XIVDeck.FFXIVPlugin.Server.Messages.Outbound;
using Buffer = System.Buffer;

namespace XIVDeck.FFXIVPlugin.Server {
    public class XIVDeckWSServer : WsServer {
        public XIVDeckWSServer(int port) : base(IPAddress.Loopback, port) {
            this.OptionDualMode = true;
        }

        protected override TcpSession CreateSession() {
            return new XIVDeckRoute(this);
        }
    }

    public class XIVDeckRoute : WsSession {
        public XIVDeckRoute(WsServer server) : base(server) { }
        
        public override void OnWsReceived(byte[] buffer, long offset, long size) {
            string rawMessage = Encoding.UTF8.GetString(buffer, (int) offset, (int) size);
            PluginLog.Debug($"Got WS message - {rawMessage}");
            
            BaseInboundMessage message = JsonConvert.DeserializeObject<BaseInboundMessage>(rawMessage);

            if (message == null) {
                PluginLog.Warning($"Got invalid message from WebSocket - {rawMessage}");
                return;
            }

            switch (message.Opcode) {
                default:
                    PluginLog.Warning($"Received message with unknown opcode: {message.Opcode}");
                    return;
                
                // system messages
                case "init":
                    message = JsonConvert.DeserializeObject<WSInitMessage>(rawMessage);
                    break;
                case "echo":
                    message = JsonConvert.DeserializeObject<WSEchoInboundMessage>(rawMessage);
                    break;
                
                // command/text
                case "command":
                    message = JsonConvert.DeserializeObject<WSChatMessage>(rawMessage);
                    break;

                // hotbar
                case "execHotbar":
                    message = JsonConvert.DeserializeObject<WSExecuteHotbarSlotOpcode>(rawMessage);
                    break;
                case "getHotbarIcon":
                    message = JsonConvert.DeserializeObject<WSGetHotbarSlotIconOpcode>(rawMessage);
                    break;
                
                // actions
                case "getUnlockedActions":
                    message = JsonConvert.DeserializeObject<WSGetUnlockedActionsOpcode>(rawMessage);
                    break;
                case "getActionIcon":
                    message = JsonConvert.DeserializeObject<WSGetActionIconOpcode>(rawMessage);
                    break;
                case "execAction":
                    message = JsonConvert.DeserializeObject<WSExecuteActionOpcode>(rawMessage);
                    break;
                
                // class switching
                case "getClasses":
                    message = JsonConvert.DeserializeObject<WSGetClassesOpcode>(rawMessage);
                    break;
                case "switchClass":
                    message = JsonConvert.DeserializeObject<WSSwitchClassOpcode>(rawMessage);
                    break;
            }

            if (message == null) {
                throw new InvalidDataException($"Message failed deserialization: {rawMessage}");
            }

            try {
                message.Process(this);
            } catch (Exception ex) {
                Injections.Chat.PrintError($"[XIVDeck] An error occured while processing a websocket message: {ex.GetType()}: {ex.Message}");
                PluginLog.Error(ex, "The WebSocket server encountered an error processing a message.");

                // Error handling logic - send an alert back to the Stream Deck so we can show a failed icon.
                if (message.SDContext != null) {
                    this.SendText(JsonConvert.SerializeObject(new WSShowSDAlert {
                        Context = message.SDContext
                    }));
                }
            }
        }

        public new void SendClose(int code, string text) {
            byte [] status= BitConverter.GetBytes((short)code);
            Array.Reverse(status);
            byte[] datas = this.Combine(status, Encoding.UTF8.GetBytes(text));
            this.SendClose(0, datas, 0, datas.Length);            
            base.Disconnect();

        }

        private byte[] Combine(byte[] first, byte[] second) {
            byte[] ret = new byte[first.Length + second.Length];
            Buffer.BlockCopy(first, 0, ret, 0, first.Length);
            Buffer.BlockCopy(second, 0, ret, first.Length, second.Length);
            return ret;
        }

        public override void OnWsConnected(HttpRequest request) {
            // only listen to requests coming to xivdeck specifically.
            if (this.Request.Url != "/xivdeck") {
                this.SendClose(1008, "Unknown request URL.");
                return;
            }

            if (this.Socket.RemoteEndPoint is not IPEndPoint point) {
                this.SendClose(1008, "Illegal remote endpoint type");
                return;
            }
            
            if (!IPAddress.IsLoopback(point.Address)) {
                this.SendClose(1008, "For security purposes, non-local connections are rejected.");
            }
        }
    }
}
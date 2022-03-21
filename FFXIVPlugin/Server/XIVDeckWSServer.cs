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

        private dynamic? Context;

        public override void OnWsReceived(byte[] buffer, long offset, long size) {
            // helps prevent totally crashing the game if the WS server doesn't know what the hell to do with a
            // message.
            
            try {
               this._processWSMessage(buffer, offset, size); 
            } catch (Exception ex) {
                PluginLog.Error("Failed reading low-level WS message", ex);
                Injections.Chat.PrintError("[XIVDeck] XIVDeck ran into a problem processing a WebSocket " +
                                           "message. If you see this message, please report a bug and attach your " +
                                           "dalamud.log file.");
            }
        }

        private void _processWSMessage(byte[] buffer, long offset, long size) {
            string rawMessage = Encoding.UTF8.GetString(buffer, (int) offset, (int) size);
            PluginLog.Debug($"Got WS message - {rawMessage}");

            JsonConvert.DeserializeObject(rawMessage, typeof(BaseInboundMessage));
            
            BaseInboundMessage? message = JsonConvert.DeserializeObject<BaseInboundMessage>(rawMessage);

            if (message == null) {
                throw new ArgumentNullException(nameof(message), $"Message decoded to null - {rawMessage}");
            }

            // FixMe: this is *hacky as hell* and will probably break badly at some point.
            // It only works for now because the WS server runs single-threaded and game-initialized messages
            // don't actually use this route.
            this.Context = message.Context;
            
            switch (message.Opcode) {
                default:
                    PluginLog.Warning($"Received message with unknown opcode: {message.Opcode}");
                    return;
                
                // system messages
                case "init":
                    message = JsonConvert.DeserializeObject<WSInitOpcode>(rawMessage);
                    break;
                case "echo":
                    message = JsonConvert.DeserializeObject<WSEchoInboundMessage>(rawMessage);
                    break;
                
                // common
                case "getIcon":
                    message = JsonConvert.DeserializeObject<WSGetIconOpcode>(rawMessage);
                    break;
                
                // command/text
                case "command":
                    message = JsonConvert.DeserializeObject<WSCommandOpcode>(rawMessage);
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
                case "getClass":
                    message = JsonConvert.DeserializeObject<WSGetClassOpcode>(rawMessage);
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
                Injections.Chat.PrintError($"[XIVDeck] {ex.Message}");
                PluginLog.Error(ex, "The WebSocket server encountered an error processing a message.");

                // Error handling logic - send an alert back to the Stream Deck so we can show a failed icon.
                this.SendText(JsonConvert.SerializeObject(new WSReplyMessage(message.Context, ex)));
            }
        }

        public void SendMessage(BaseOutboundMessage message) {
            if (this.Context != null) {
                message.Context = this.Context;
            }
            
            this.SendText(JsonConvert.SerializeObject(message));
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
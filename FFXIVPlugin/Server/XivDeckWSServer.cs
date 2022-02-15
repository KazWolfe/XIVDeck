using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Dalamud.Logging;
using FFXIVPlugin.helpers;
using FFXIVPlugin.Server.Messages.Inbound;
using NetCoreServer;
using Newtonsoft.Json;
using XivCommon;

namespace FFXIVPlugin.Server {
    public class XivDeckWSServer : WsServer {
        public XivDeckWSServer(int port) : base(IPAddress.Loopback, port) { }

        protected override TcpSession CreateSession() {
            return new XivDeckRoute(this);
        }
    }

    public class XivDeckRoute : WsSession {

        public XivDeckRoute(WsServer server) : base(server) { }

        public override void OnWsReceived(byte[] buffer, long offset, long size) {
            string rawMessage = Encoding.UTF8.GetString(buffer, (int) offset, (int) size);
            BaseInboundMessage message = JsonConvert.DeserializeObject<BaseInboundMessage>(rawMessage);

            switch (message.Opcode) {
                case "command":
                    message = JsonConvert.DeserializeObject<WSCommandInboundMessage>(rawMessage);
                    break;
                case "echo":
                    message = JsonConvert.DeserializeObject<WSEchoInboundMessage>(rawMessage);
                    break;
                case "execHotbar":
                    message = JsonConvert.DeserializeObject<WSExecuteHotbarSlotOpcode>(rawMessage);
                    break;
                case "getHotbarIcon":
                    message = JsonConvert.DeserializeObject<WSGetHotbarSlotIconOpcode>(rawMessage);
                    break;
                case "init":
                    message = JsonConvert.DeserializeObject<WSInitMessage>(rawMessage);
                    break;
            }

            message.Process(this);
        }

        public new void SendClose(int code, string text) {
            byte [] status= BitConverter.GetBytes((short)code);
            Array.Reverse(status);
            byte[] datas = Combine(status, Encoding.UTF8.GetBytes(text));
            SendClose(0, datas, 0, datas.Length);            
            base.Disconnect();

        }

        private byte[] Combine(byte[] first, byte[] second) {
            byte[] ret = new byte[first.Length + second.Length];
            System.Buffer.BlockCopy(first, 0, ret, 0, first.Length);
            System.Buffer.BlockCopy(second, 0, ret, first.Length, second.Length);
            return ret;
        }

        public override void OnWsConnected(HttpRequest request) {
            // only listen to requests coming to xivdeck specifically.
            if (this.Request.Url != "/xivdeck") {
                this.SendClose(1003, "Unknown request URL.");
                return;
            }
            
            if (!IPAddress.IsLoopback(((IPEndPoint) this.Socket.RemoteEndPoint).Address)) {
                this.SendClose(1008, "For security purposes, non-local connections are rejected.");
                return;
            }
        }
    }
}
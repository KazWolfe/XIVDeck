using System;
using System.Collections.Generic;
using System.Reflection;
using FFXIVPlugin.helpers;
using Newtonsoft.Json;
using WebSocketSharp;

namespace FFXIVPlugin.Server.Messages.Inbound {
    public class WSInitMessage : BaseInboundMessage {
        public string Data { get; set; }

        public override void Process(WebSocket socket) {
            var reply = new Dictionary<string, string>();
            reply["messageType"] = "initReply";
            reply["version"] = Assembly.GetExecutingAssembly().GetName().Version.ToString();

            socket.Send(JsonConvert.SerializeObject(reply));

            // check for first-run
            if (!XIVDeckPlugin.Instance.Configuration.HasLinkedStreamDeckPlugin) {
                Injections.Chat.Print("[XIVDeck] Thank you for installing the Stream Deck plugin. XIVDeck is " +
                                      "now ready to go!");

                XIVDeckPlugin.Instance.Configuration.HasLinkedStreamDeckPlugin = true;
                XIVDeckPlugin.Instance.Configuration.Save();
            }
        }

        public WSInitMessage() : base("init") { }
    }
}
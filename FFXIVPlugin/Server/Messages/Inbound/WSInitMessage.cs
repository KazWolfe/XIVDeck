using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Dalamud.Logging;
using NetCoreServer;
using Newtonsoft.Json;
using XIVDeck.FFXIVPlugin.Base;

namespace XIVDeck.FFXIVPlugin.Server.Messages.Inbound {
    [SuppressMessage("ReSharper", "UnassignedGetOnlyAutoProperty", Justification = "JSON serializer will initialize values")]
    public class WSInitMessage : BaseInboundMessage {
        public string Data { get; set; } = default!;
        public string Version { get; set; } = default!;

        public override void Process(WsSession session) {
            if (System.Version.Parse(this.Version) < System.Version.Parse(Constants.MinimumSDPluginVersion)) {
                session.SendClose(1008, "The version of the Stream Deck plugin is too old.");
                PluginLog.Warning($"The currently-installed version of the XIVDeck Stream Deck plugin " +
                                  $"is {this.Version}, but version {Constants.MinimumSDPluginVersion} is needed.");
                return;
            }
            
            var reply = new Dictionary<string, string> {
                ["messageType"] = "initReply",
                ["version"] = Assembly.GetExecutingAssembly().GetName().Version!.ToString()
            };

            session.SendTextAsync(JsonConvert.SerializeObject(reply));

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
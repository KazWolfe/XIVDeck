using System.Reflection;
using System.Threading.Tasks;
using Dalamud.Logging;
using EmbedIO.WebSockets;
using XIVDeck.FFXIVPlugin.Base;
using XIVDeck.FFXIVPlugin.Server.Helpers;
using XIVDeck.FFXIVPlugin.Server.Messages.Outbound;
using XIVDeck.FFXIVPlugin.UI.Windows;
using XIVDeck.FFXIVPlugin.UI.Windows.Nags;
using XIVDeck.FFXIVPlugin.Utils;

namespace XIVDeck.FFXIVPlugin.Server.Messages.Inbound;

[WSOpcode("init")]
public class WSInitOpcode : BaseInboundMessage {
    public string Version { get; set; } = default!;
    
    public override async Task Process(IWebSocketContext context) {
        // hide all nags
        NagWindow.CloseAllNags();
        
        if (System.Version.Parse(this.Version) < System.Version.Parse(Constants.MinimumSDPluginVersion)) {
            await context.WebSocket.CloseAsync(CloseStatusCode.ProtocolError,
                "The version of the Stream Deck plugin is too old.", context.CancellationToken);
            
            PluginLog.Warning($"The currently-installed version of the XIVDeck Stream Deck plugin " +
                              $"is {this.Version}, but version {Constants.MinimumSDPluginVersion} is needed.");
            ForcedUpdateNag.Show();

            return;
        }

        var xivPluginVersion = Assembly.GetExecutingAssembly().GetName().Version!.ToString();
        var reply = new WSInitReplyMessage(xivPluginVersion, AuthHelper.Instance.Secret);
        await WebUtils.SendMessage(context, reply);
        PluginLog.Information($"XIVDeck Stream Deck Plugin version {this.Version} has connected!");

        // check for first-run
        if (!XIVDeckPlugin.Instance.Configuration.HasLinkedStreamDeckPlugin) {
            Injections.Chat.Print("[XIVDeck] Thank you for installing the Stream Deck plugin. XIVDeck is " +
                                  "now ready to go!");

            XIVDeckPlugin.Instance.Configuration.HasLinkedStreamDeckPlugin = true;
            XIVDeckPlugin.Instance.Configuration.Save();
        }
    }
}
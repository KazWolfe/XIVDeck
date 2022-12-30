using System.Reflection;
using System.Threading.Tasks;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Logging;
using EmbedIO.WebSockets;
using Newtonsoft.Json;
using XIVDeck.FFXIVPlugin.Base;
using XIVDeck.FFXIVPlugin.Game.Chat;
using XIVDeck.FFXIVPlugin.Resources.Localization;
using XIVDeck.FFXIVPlugin.Server.Helpers;
using XIVDeck.FFXIVPlugin.Server.Messages.Outbound;
using XIVDeck.FFXIVPlugin.UI;
using XIVDeck.FFXIVPlugin.UI.Windows;
using XIVDeck.FFXIVPlugin.UI.Windows.Nags;
using XIVDeck.FFXIVPlugin.Utils;

namespace XIVDeck.FFXIVPlugin.Server.Messages.Inbound;

public enum PluginMode {
    Plugin,
    Inspector,
    Developer
}

[WSOpcode("init")]
public class WSInitOpcode : BaseInboundMessage {
    public string Version { get; set; } = default!;
    [JsonProperty("mode")]
    public PluginMode? Mode { get; set; }
    
    public override async Task Process(IWebSocketContext context) {
        // hide all nags
        NagWindow.CloseAllNags();
        
        var sdPluginVersion = System.Version.Parse(this.Version);
        
        if (sdPluginVersion < System.Version.Parse(Constants.MinimumSDPluginVersion)) {
            await context.WebSocket.CloseAsync(CloseStatusCode.ProtocolError,
                "The version of the Stream Deck plugin is too old.", context.CancellationToken);
            
            PluginLog.Warning("The currently-installed version of the XIVDeck Stream Deck plugin " +
                              $"is {this.Version}, but version {Constants.MinimumSDPluginVersion} is needed.");
            ForcedUpdateNag.Show();

            return;
        }

        var xivPluginVersion = Assembly.GetExecutingAssembly().GetName().Version!;
        var reply = new WSInitReplyMessage(xivPluginVersion.GetMajMinBuild(), AuthHelper.Instance.Secret);  
        await context.SendMessage(reply);
        PluginLog.Information($"XIVDeck Stream Deck Plugin ({this.Mode}) version {this.Version} has connected!");

        // version check
        if (sdPluginVersion.IsOlderThan(xivPluginVersion) && (this.Mode is null or PluginMode.Plugin)) {
            var updateMessage = new SeStringBuilder()
                .Append(ErrorNotifier.BuildPrefixedString(""))
                .AddText("Your version of the XIVDeck Stream Deck Plugin is out of date. Please consider installing ")
                .Add(ChatLinkWiring.GetPayload(LinkCode.GetGithubReleaseLink))
                .AddUiForeground($"\xE0BB version {xivPluginVersion.GetMajMinBuild()}", 32)
                .Add(RawPayload.LinkTerminator)
                .AddText(" from GitHub!")
                .Build();
            
            DeferredChat.SendOrDeferMessage(updateMessage);
        }

        // check for first-run
        if (!XIVDeckPlugin.Instance.Configuration.HasLinkedStreamDeckPlugin) {
            Injections.Chat.Print(ErrorNotifier.BuildPrefixedString(UIStrings.WSInitOpcode_ThanksForInstall));

            XIVDeckPlugin.Instance.Configuration.HasLinkedStreamDeckPlugin = true;
            XIVDeckPlugin.Instance.Configuration.Save();
        }
    }
}
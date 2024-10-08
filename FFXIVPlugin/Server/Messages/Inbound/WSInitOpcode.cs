﻿using System;
using System.Reflection;
using System.Threading.Tasks;
using Dalamud.Interface.ImGuiNotification;
using EmbedIO.WebSockets;
using Newtonsoft.Json;
using XIVDeck.FFXIVPlugin.Base;
using XIVDeck.FFXIVPlugin.Game.Chat;
using XIVDeck.FFXIVPlugin.Resources.Localization;
using XIVDeck.FFXIVPlugin.Server.Helpers;
using XIVDeck.FFXIVPlugin.Server.Messages.Outbound;
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
    [JsonProperty("mode")] public PluginMode? Mode { get; set; }

    public override async Task Process(IWebSocketContext context) {
        // hide all nags
        NagWindow.CloseAllNags();

        var sdPluginVersion = System.Version.Parse(this.Version);

        if (sdPluginVersion < System.Version.Parse(Constants.MinimumSDPluginVersion)) {
            await context.WebSocket.CloseAsync(CloseStatusCode.ProtocolError,
                "The version of the Stream Deck plugin is too old.", context.CancellationToken);

            Injections.PluginLog.Warning("The currently-installed version of the XIVDeck Stream Deck plugin " +
                                         $"is {this.Version}, but version {Constants.MinimumSDPluginVersion} is needed.");
            ForcedUpdateNag.Show();

            return;
        }

        var xivPluginVersion = Assembly.GetExecutingAssembly().GetName().Version!.StripRevision();
        var reply = new WSInitReplyMessage(xivPluginVersion.GetMajMinBuild(), AuthHelper.Instance.Secret);
        await context.SendMessage(reply);
        Injections.PluginLog.Information(
            $"XIVDeck Stream Deck Plugin ({this.Mode}) version {this.Version} has connected!");

        // version check behavior
        if (Injections.PluginInterface is { IsDev: false } && this.Mode is PluginMode.Plugin) {
            if (Injections.PluginInterface is { IsTesting: true } && sdPluginVersion < xivPluginVersion) {
                TestingUpdateNag.Show();
            } else if (sdPluginVersion < xivPluginVersion) {
                var n = Injections.NotificationManager.AddNotification(new Notification {
                    Title = UIStrings.WSInitOpcode_PluginUpdateAvailableNotificationTitle,
                    Content = string.Format(UIStrings.WSInitOpcode_SDPluginUpdateNotificationBody, sdPluginVersion,
                        xivPluginVersion),
                    Type = NotificationType.Warning,
                    Minimized = false,
                    RespectUiHidden = false,
                    InitialDuration = TimeSpan.FromSeconds(20),
                });

                n.Click += _ => { UiUtil.OpenXIVDeckGitHub($"/releases/tag/v{VersionUtils.GetCurrentMajMinBuild()}"); };
            } else if (sdPluginVersion > xivPluginVersion) {
                var n = Injections.NotificationManager.AddNotification(new Notification {
                    Title = UIStrings.WSInitOpcode_PluginUpdateAvailableNotificationTitle,
                    Content = UIStrings.WSInitOpcode_GamePluginOutdated,
                    Type = NotificationType.Warning,
                    Minimized = false,
                    RespectUiHidden = false,
                    InitialDuration = TimeSpan.FromSeconds(20),
                });

                n.Click += _ => {
                    Injections.PluginInterface.OpenPluginInstallerTo(
                        searchText: Injections.PluginInterface.InternalName);
                };
            }
        }

        // check for first-run
        if (!XIVDeckPlugin.Instance.Configuration.HasLinkedStreamDeckPlugin) {
            Injections.Chat.Print(ErrorNotifier.BuildPrefixedString(UIStrings.WSInitOpcode_ThanksForInstall));

            XIVDeckPlugin.Instance.Configuration.HasLinkedStreamDeckPlugin = true;
            XIVDeckPlugin.Instance.Configuration.Save();
        }
    }
}

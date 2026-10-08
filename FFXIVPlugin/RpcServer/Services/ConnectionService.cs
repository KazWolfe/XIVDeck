using System;
using Dalamud.Plugin;
using Serilog;
using XIVDeck.FFXIVPlugin.Config;
using XIVDeck.FFXIVPlugin.RpcServer.Helpers;
using XIVDeck.FFXIVPlugin.Contract;
using XIVDeck.FFXIVPlugin.UI;
using XIVDeck.FFXIVPlugin.UI.Windows.Nags;
using XIVDeck.FFXIVPlugin.Utils;

namespace XIVDeck.FFXIVPlugin.RpcServer.Services;

[RpcService("Connection")]
public class ConnectionService(ILogger log, UIManager uiManager, UpdateNotifier updateNotifier, ConfigService config,
    IDalamudPluginInterface pluginInterface, RpcClient client) {
    public ServerHello Initialize(ClientHello clientHello) {
        // any client connecting means we can talk, no need to setup nag.
        uiManager.CloseAll<SetupNag>();

        if (!config.Config.HasLinkedStreamDeckPlugin) {
            config.Config.HasLinkedStreamDeckPlugin = true;
            config.Save();
        }

        if (!this.DetermineCompatibility(clientHello)) {
            log.Warning("Rejecting JSON-RPC client, incompatible version '{Version}'.", clientHello.ClientVersion);

            client.DisconnectAfterResponse();
            return new ServerHello { Accepted = false };
        }

        updateNotifier.DismissForcedUpdate();
        client.MountAll();

        return new ServerHello {
            Accepted = true,
            FfxivPluginVersion = VersionUtils.GetCurrentMajMinBuild(),
            ApiVersion = VersionUtils.GetCurrentVersion().GetCompatibilityLevel(),
        };
    }

    public string Ping() {
        return "Pong";
    }

    private bool DetermineCompatibility(ClientHello clientHello) {
        if (!Version.TryParse(clientHello.ClientVersion, out var clientVersion)) {
            log.Warning("Client sent an unparseable version '{Version}' - treating as forced update required.",
                clientHello.ClientVersion);
            updateNotifier.ShowForcedUpdate();
            return false;
        }

        var currentVersion = VersionUtils.GetCurrentVersion();
        clientVersion = clientVersion.StripRevision();

        if (!clientVersion.IsCompatibleWith(currentVersion)) {
            updateNotifier.ShowForcedUpdate();
            return false;
        }

        if (pluginInterface.IsTesting && clientVersion != currentVersion) {
            updateNotifier.ShowTestingUpdate(currentVersion);
        } else {
            updateNotifier.DismissTestingUpdate();
        }

        return true;
    }
}

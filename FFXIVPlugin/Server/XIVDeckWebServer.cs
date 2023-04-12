using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Dalamud.Logging;
using EmbedIO;
using XIVDeck.FFXIVPlugin.Exceptions;
using XIVDeck.FFXIVPlugin.Game.Chat;
using XIVDeck.FFXIVPlugin.Resources.Localization;
using XIVDeck.FFXIVPlugin.Server.Helpers;
using XIVDeck.FFXIVPlugin.Server.Messages;
using XIVDeck.FFXIVPlugin.Utils;

namespace XIVDeck.FFXIVPlugin.Server;

public class XIVDeckWebServer : IXIVDeckServer {
    private readonly PluginLogShim _logShim = new();
    private readonly XIVDeckPlugin _plugin;

    // Core web server components
    private IWebServer? _host;
    private CancellationTokenSource? _cts;
    private Task? _serverTask;

    // Exposed modules
    private XIVDeckWSServer? _wsServer;

    public XIVDeckWebServer(XIVDeckPlugin plugin) {
        Swan.Logging.Logger.RegisterLogger(this._logShim);
        this._plugin = plugin;
    }

    public bool IsRunning => this._host != null && this._host.State != WebServerState.Stopped;

    public void StartServer() {
        this._cts = new CancellationTokenSource();

        var listenerMode = GetListenerMode();
        var port = this._plugin.Configuration.WebSocketPort;

        PluginLog.Debug($"Starting EmbedIO server on port {port} with listener mode {listenerMode}");

        // FIXME: the EmbedIO listener mode has a problem where "localhost" only resolves to IPv6.
        // If localhost is somehow resolved to 127.0.0.1, all communication will fail for no apparent reason.
        this._host = new WebServer(o => o
            .WithUrlPrefixes(GenerateUrlPrefixes(port))
            .WithMode(listenerMode)
        );

        this._wsServer = new XIVDeckWSServer("/ws");

        this._host.WithCors(origins: "file://");
        this._host.WithModule(this._wsServer);
        this._host.WithModule(new AuthModule("/"));

        this._host.StateChanged += (_, e) => {
            PluginLog.Debug($"EmbedIO server changed state to {e.NewState.ToString()}");
        };

        ConfigureErrorHandlers(this._host);
        ApiControllerWiring.Autowire(this._host);

        this._serverTask = Task.Run(async () => {
            try {
                await this._host.RunAsync(this._cts.Token);
            } catch (HttpListenerException ex) when (ex.ErrorCode == 32) {
                PluginLog.Warning(ex, "Port was already in use!");
            } catch (Exception ex) {
                PluginLog.Error(ex, "Error during webserver run!");
                ErrorNotifier.ShowError(UIStrings.XIVDeckWebServer_RunException, false, true, true);
            }
        }, this._cts.Token);
    }

    public void StopServer() {
        this._cts?.Cancel();
        this._serverTask?.Wait();
        this._host?.Dispose();

        PluginLog.Debug("Web server has been stopped.");
    }

    public void Dispose() {
        this.StopServer();

        Swan.Logging.Logger.UnregisterLogger(this._logShim);
        this._logShim.Dispose();

        GC.SuppressFinalize(this);
    }

    public void BroadcastMessage(BaseOutboundMessage message) {
        this._wsServer?.BroadcastMessage(message);
    }

    private static HttpListenerMode GetListenerMode() {
        if (XIVDeckPlugin.Instance.Configuration.HttpListenerMode != null) {
            var mode = XIVDeckPlugin.Instance.Configuration.HttpListenerMode.Value;
            PluginLog.Debug($"Config explicitly set HttpListenerMode to {mode}.");
            return mode;
        }

        if (Dalamud.Utility.Util.IsLinux()) {
            PluginLog.Information("Linux environment detected; using EmbedIO listener.");
            return HttpListenerMode.EmbedIO;
        }

        PluginLog.Debug("HttpListenerMode not set; using default Microsoft listener.");
        return HttpListenerMode.Microsoft;
    }

    private static string[] GenerateUrlPrefixes(int port) {
        if (XIVDeckPlugin.Instance.Configuration.ListenOnAllInterfaces) {
            PluginLog.Warning("XIVDeck is configured to listen on all interfaces! THIS IS A SECURITY RISK!");
            return new[] { "http://*:${port}" };
        }

        var prefixes = new List<string> {$"http://localhost:{port}"};

        // Add in explicit URL prefixes for v4 (and v6) because HttpListener just doesn't let us grab all traffic on an interface.
        if (NetworkUtil.HostSupportsLocalIPv4()) prefixes.Add($"http://127.0.0.1:{port}");
        if (NetworkUtil.HostSupportsLocalIPv6()) prefixes.Add($"http://[::1]:{port}");
        
        return prefixes.ToArray();
    }

    private static void ConfigureErrorHandlers(IWebServer server) {
        server.OnUnhandledException = (ctx, ex) => {
            // Handle known exception types first, as these can be thrown by various subsystems
            switch (ex) {
                case ActionLockedException:
                    return server.OnHttpException(ctx, HttpException.Forbidden(ex.Message, ex));

                case PlayerNotLoggedInException:
                case IllegalGameStateException:
                    return server.OnHttpException(ctx, HttpException.BadRequest(ex.Message, ex));

                case ActionNotFoundException:
                    return server.OnHttpException(ctx, HttpException.NotFound(ex.Message, ex));
            }

            // And then fallback to unknown exceptions
            PluginLog.Error(ex, "Unhandled exception while processing request: " +
                                $"{ctx.Request.HttpMethod} {ctx.Request.Url.PathAndQuery}");
            ErrorNotifier.ShowError(ex.Message, debounce: true);
            return ExceptionHandler.Default(ctx, ex);
        };

        server.OnHttpException = (ctx, ex) => {
            var inner = ex.DataObject as Exception ?? (HttpException) ex;

            PluginLog.Warning(inner, $"Got HTTP {ex.StatusCode} while processing request: " +
                                     $"{ctx.Request.HttpMethod} {ctx.Request.Url.PathAndQuery}");

            // Only show messages to users if it's a POST request (button action)
            if (ctx.Request.HttpVerb == HttpVerbs.Post) {
                ErrorNotifier.ShowError(ex.Message ?? inner.Message, true);
            }

            return HttpExceptionHandler.Default(ctx, ex);
        };
    }
}
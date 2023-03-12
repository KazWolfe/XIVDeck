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

namespace XIVDeck.FFXIVPlugin.Server;

public class XIVDeckWebServer : IDisposable {
    private readonly IWebServer _host;
    private readonly CancellationTokenSource _cts = new();

    private readonly PluginLogShim _logShim = new();

    private Task? _serverTask;

    public XIVDeckWebServer(XIVDeckPlugin plugin) {
        Swan.Logging.Logger.RegisterLogger(this._logShim);

        var listenerMode = plugin.Configuration.HttpListenerMode;
        var port = plugin.Configuration.WebSocketPort;

        PluginLog.Debug($"Starting EmbedIO server on port {port} with listener mode {listenerMode}");

        // FIXME: the EmbedIO listener mode has a problem where "localhost" only resolves to IPv6.
        // If localhost is somehow resolved to 127.0.0.1, all communication will fail for no apparent reason.
        this._host = new WebServer(o => o
            .WithUrlPrefixes(GenerateUrlPrefixes(port))
            .WithMode(listenerMode)
        );

        this._host.WithCors(origins: "file://");
        this._host.WithModule(new XIVDeckWSServer("/ws"));
        this._host.WithModule(new AuthModule("/"));

        this._host.StateChanged += (_, e) => {
            PluginLog.Debug($"EmbedIO server changed state to {e.NewState.ToString()}");
        };

        this.ConfigureErrorHandlers();
        ApiControllerWiring.Autowire(this._host);
    }

    public bool IsRunning => (this._host.State != WebServerState.Stopped);

    public void StartServer() {
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

    public void Dispose() {
        this._cts.Cancel();
        this._serverTask?.Wait();
        this._host.Dispose();

        Swan.Logging.Logger.UnregisterLogger(this._logShim);
        this._logShim.Dispose();

        GC.SuppressFinalize(this);
    }

    private static string[] GenerateUrlPrefixes(int port) {
        var prefixes = new List<string> {$"http://localhost:{port}", $"http://127.0.0.1:{port}"};

        if (Socket.OSSupportsIPv6)
            prefixes.Add($"http://[::1]:{port}");

        if (XIVDeckPlugin.Instance.Configuration.ListenOnAllInterfaces) {
            PluginLog.Warning("XIVDeck is configured to listen on all interfaces!!");
            prefixes.Add($"http://*:{port}");
        }

        return prefixes.ToArray();
    }

    private void ConfigureErrorHandlers() {
        this._host.OnUnhandledException = (ctx, ex) => {
            // Handle known exception types first, as these can be thrown by various subsystems
            switch (ex) {
                case ActionLockedException:
                    throw HttpException.Forbidden(ex.Message, ex);

                case PlayerNotLoggedInException:
                case IllegalGameStateException:
                    throw HttpException.BadRequest(ex.Message, ex);

                case ActionNotFoundException:
                    throw HttpException.NotFound(ex.Message, ex);
            }

            // And then fallback to unknown exceptions
            PluginLog.Error(ex, "Unhandled exception while processing request: " +
                                $"{ctx.Request.HttpMethod} {ctx.Request.Url.PathAndQuery}");
            ErrorNotifier.ShowError(ex.Message, debounce: true);
            return ExceptionHandler.Default(ctx, ex);
        };

        this._host.OnHttpException = (ctx, ex) => {
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
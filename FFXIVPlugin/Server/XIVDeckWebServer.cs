using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using Dalamud.Logging;
using EmbedIO;
using XIVDeck.FFXIVPlugin.Exceptions;
using XIVDeck.FFXIVPlugin.Game;
using XIVDeck.FFXIVPlugin.Game.Chat;
using XIVDeck.FFXIVPlugin.Server.Helpers;

namespace XIVDeck.FFXIVPlugin.Server;

public class XIVDeckWebServer : IDisposable {
    private readonly IWebServer _host;
    private readonly CancellationTokenSource _cts = new();

    public XIVDeckWebServer(int port) {
        this._host = new WebServer(o => o
            .WithUrlPrefixes(GenerateUrlPrefixes(port))
            .WithMode(HttpListenerMode.Microsoft)
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
        this._host.Start(this._cts.Token);
    }

    public void Dispose() {
        this._cts.Cancel();
        GC.SuppressFinalize(this);
    }

    private static string[] GenerateUrlPrefixes(int port) {
        var prefixes = new List<string> { $"http://localhost:{port}", $"http://127.0.0.1:{port}" };
        
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
            PluginLog.Error(ex, $"Unhandled exception while processing request: " +
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

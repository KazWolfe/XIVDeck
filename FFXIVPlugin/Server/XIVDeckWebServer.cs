using System;
using Dalamud.Logging;
using EmbedIO;
using XIVDeck.FFXIVPlugin.Exceptions;
using XIVDeck.FFXIVPlugin.Game;
using XIVDeck.FFXIVPlugin.Resources.Localization;
using XIVDeck.FFXIVPlugin.Server.Helpers;

namespace XIVDeck.FFXIVPlugin.Server;

public class XIVDeckWebServer : IDisposable {
    private readonly IWebServer _host;
    
    public XIVDeckWebServer(int port) {
        this._host = new WebServer(o => o
            .WithUrlPrefixes(GenerateUrlPrefixes(port))
            .WithMode(HttpListenerMode.EmbedIO)
        );
        
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
        this._host.Start();
    }

    public void Dispose() {
        this._host.Dispose();

        GC.SuppressFinalize(this);
    }

    private static string[] GenerateUrlPrefixes(int port) {
        return new[] { $"http://localhost:{port}", $"http://127.0.0.1:{port}", $"http://[::1]:{port}" };
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
            PluginLog.Error(ex, $"Unhandled exception while processing request: {ctx.Request.HttpMethod} {ctx.Request.Url.PathAndQuery}");
            ErrorNotifier.ShowError($"[{string.Format(UIStrings.ErrorHandler_ErrorPrefix, UIStrings.XIVDeck)}] {ex.Message}");
            return ExceptionHandler.Default(ctx, ex);
        };

        this._host.OnHttpException = (ctx, ex) => {
            var inner = (Exception?) ex.DataObject ?? (Exception) ex;
            PluginLog.Warning(inner, $"Got HTTP {ex.StatusCode} while processing request: {ctx.Request.HttpMethod} {ctx.Request.Url.PathAndQuery}");

            // Only show messages to users if it's a POST request (button action)
            if (ctx.Request.HttpVerb == HttpVerbs.Post) {
                ErrorNotifier.ShowError($"[{UIStrings.XIVDeck}] {ex.Message}", true);
            }
            
            return HttpExceptionHandler.Default(ctx, ex);
        };
    }
}

using System;
using Dalamud.Logging;
using EmbedIO;
using XIVDeck.FFXIVPlugin.Game;
using XIVDeck.FFXIVPlugin.Server.Helpers;

namespace XIVDeck.FFXIVPlugin.Server;

public class XIVDeckWebServer : IDisposable {
    private readonly IWebServer _host;
    
    public XIVDeckWebServer(int port) {
        this._host = new WebServer(o => o
            .WithUrlPrefixes(GenerateUrlPrefixes(port))
            .WithMode(HttpListenerMode.Microsoft)
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
            PluginLog.Error(ex, $"Unhandled exception while processing request: {ctx.Request.HttpMethod} {ctx.Request.Url.PathAndQuery}");
            ErrorNotifier.ShowError($"[XIVDeck - ERROR] {ex.Message}");
            return ExceptionHandler.Default(ctx, ex);
        };

        this._host.OnHttpException = (ctx, ex) => {
            PluginLog.Warning((Exception) ex, $"Got HTTP {ex.StatusCode} while processing request: {ctx.Request.HttpMethod} {ctx.Request.Url.PathAndQuery}");

            // Only show messages to users if it's a POST request (button action)
            if (ctx.Request.HttpVerb == HttpVerbs.Post) {
                ErrorNotifier.ShowError($"[XIVDeck] {ex.Message}", true);
            }
            
            return HttpExceptionHandler.Default(ctx, ex);
        };
    }
}

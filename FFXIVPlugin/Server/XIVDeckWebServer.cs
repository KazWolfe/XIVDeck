using System;
using Dalamud.Logging;
using EmbedIO;
using XIVDeck.FFXIVPlugin.Base;
using XIVDeck.FFXIVPlugin.Server.Helpers;
using XIVDeck.FFXIVPlugin.Utils;

namespace XIVDeck.FFXIVPlugin.Server;

public class XIVDeckWebServer : IDisposable {
    private readonly IWebServer _host;
    private readonly XIVDeckWSServer _xivDeckWSModule;
    
    public XIVDeckWebServer(int port) {
        this._xivDeckWSModule = new XIVDeckWSServer("/ws");
        
        this._host = new WebServer(o => o
            .WithUrlPrefixes(GenerateUrlPrefixes(port))
            .WithMode(HttpListenerMode.EmbedIO)
        );
            
        this._host.WithModule(this._xivDeckWSModule);
        this._host.WithModule(new AuthModule("/"));
        
        this._host.OnAny(context => {
            PluginLog.Debug($"Got HTTP request {context.Request.HttpMethod} {context.Request.Url.PathAndQuery}");
            throw RequestHandler.PassThrough();
        });
        
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
        this._xivDeckWSModule.Dispose();
        this._host.Dispose();
    }

    private static string[] GenerateUrlPrefixes(int port) {
        return new[] { $"http://localhost:{port}", $"http://127.0.0.1:{port}", $"http://[::1]:{port}" };
    }

    private void ConfigureErrorHandlers() {
        this._host.OnUnhandledException = (ctx, ex) => {
            PluginLog.Error(ex, $"Unhandled exception while processing request: {ctx.Request.HttpMethod} {ctx.Request.Url.PathAndQuery}");
            Injections.Chat.PrintError($"[XIVDeck - ERROR] {ex.Message}");
            return ExceptionHandler.Default(ctx, ex);
        };

        this._host.OnHttpException = (ctx, ex) => {
            PluginLog.Warning((Exception) ex, $"Got HTTP {ex.StatusCode} while processing request: {ctx.Request.HttpMethod} {ctx.Request.Url.PathAndQuery}");

            // Bit of an ugly shim, but allows us to suppress messages related to auth
            if (ex.StatusCode != 401) {
                Injections.Chat.PrintError($"[XIVDeck] {ex.Message}");
            }
            
            return HttpExceptionHandler.Default(ctx, ex);
        };
    }
}

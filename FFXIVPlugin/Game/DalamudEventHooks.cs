using System;
using Dalamud.Plugin.Services;
using Serilog;
using XIVDeck.FFXIVPlugin.IoC;

namespace XIVDeck.FFXIVPlugin.Game;

[Service(ServiceFlags.Singleton | ServiceFlags.AutoLoad)]
public class DalamudEventHooks : IDisposable {
    private readonly ILogger _log;
    private readonly IClientState _clientState;

    public DalamudEventHooks(ILogger log, IClientState clientState) {
        this._log = log;
        this._clientState = clientState;

        this._clientState.Login += this.OnLogin;
    }

    public void Dispose() {
        this._clientState.Login -= this.OnLogin;
    }

    private void OnLogin() {
        // TODO: Clear icon caches, reload all SD actions.
    }
}

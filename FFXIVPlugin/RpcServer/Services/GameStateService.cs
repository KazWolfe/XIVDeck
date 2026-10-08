using System;
using XIVDeck.FFXIVPlugin.Contract;
using XIVDeck.FFXIVPlugin.Game.Watchers;
using XIVDeck.FFXIVPlugin.RpcServer.Helpers;
using XIVDeck.FFXIVPlugin.Utils;

namespace XIVDeck.FFXIVPlugin.RpcServer.Services;

[RpcService("GameState")]
public class GameStateService : IDisposable {
    private readonly FocusWatcher _focusWatcher;

    /// <summary>
    /// The game window gained focus. Losing focus is not reported; use <see cref="GetFocusState"/> for the current
    /// state.
    /// </summary>
    public event EventHandler<FocusState>? FocusChanged;

    public GameStateService(FocusWatcher focusWatcher) {
        this._focusWatcher = focusWatcher;
        this._focusWatcher.FocusChanged += this.OnFocusChanged;
    }

    public void Dispose() {
        this._focusWatcher.FocusChanged -= this.OnFocusChanged;

        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// The current focus state. Clients call this on connect, since <see cref="FocusChanged"/> only reports changes.
    /// </summary>
    public FocusState GetFocusState() {
        return new FocusState {
            IsFocused = this._focusWatcher.IsFocused,
            LastFocusTime = this._focusWatcher.LastFocusTime?.ToUnixTimeMilliseconds(),
        };
    }

    private void OnFocusChanged(object? sender, FocusState state) {
        if (state.IsFocused) {
            this.FocusChanged?.InvokeSafely(this, state);
        }
    }
}

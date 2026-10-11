using System;
using System.Runtime.InteropServices;
using System.Threading;
using Dalamud.Hooking;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using Serilog;
using XIVDeck.FFXIVPlugin.Contract;
using XIVDeck.FFXIVPlugin.IoC;
using XIVDeck.FFXIVPlugin.Utils;

namespace XIVDeck.FFXIVPlugin.Game.Watchers;

[Service(ServiceFlags.Singleton)]
public unsafe class FocusWatcher : IDisposable {
    public event EventHandler<FocusState>? FocusChanged;

    private readonly ILogger _log;
    private readonly Hook<Framework.Delegates.SetInactive>? _setInactiveHook;

    private bool _focusedCache;

    // unix ms focus was last asserted, 0 if never. written on the game thread, read from anywhere.
    private long _lastFocusTime;

    /// <summary>
    /// Checks (live) if the game is currently marked as focused.
    /// </summary>
    public bool IsFocused => this.GetFocusState();

    /// <summary>
    /// The last time focus was known to be asserted (on load, or by the game gaining focus), or null if it hasn't been
    /// since the plugin loaded.
    /// </summary>
    public DateTimeOffset? LastFocusTime {
        get {
            var time = Interlocked.Read(ref this._lastFocusTime);
            return time == 0 ? null : DateTimeOffset.FromUnixTimeMilliseconds(time);
        }
    }

    public FocusWatcher(ILogger log, IGameInteropProvider interop) {
        this._log = log;

        this._setInactiveHook = interop.HookFromAddress<Framework.Delegates.SetInactive>(
            Framework.Addresses.SetInactive.Value,
            this.SetInactiveDetour);
        this._setInactiveHook?.Enable();

        this.SetFocusState(this.GetFocusState());
    }

    public void Dispose() {
        this._setInactiveHook?.Dispose();

        GC.SuppressFinalize(this);
    }

    private void SetInactiveDetour(Framework* framework, bool inactive) {
        this._setInactiveHook!.OriginalDisposeSafe(framework, inactive);
        this.SetFocusState(!inactive);
    }

    private bool GetFocusState() {
        // thread-safe: WindowInactive is set via an atomic operation.
        var framework = Framework.Instance();
        return framework != null && !framework->WindowInactive;
    }

    private void SetFocusState(bool focused) {
        if (this._focusedCache == focused) return;

        this._log.Verbose("New focus state: {focus}", focused);

        this._focusedCache = focused;
        if (focused) {
            Interlocked.Exchange(ref this._lastFocusTime, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
        }

        this.FocusChanged?.InvokeSafely(this, new FocusState {
            IsFocused = focused,
            LastFocusTime = this.LastFocusTime?.ToUnixTimeMilliseconds(),
        });
    }
}

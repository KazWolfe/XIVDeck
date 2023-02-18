using System;

namespace XIVDeck.FFXIVPlugin.Utils; 

public class DisposableWrapper : IDisposable {
    private readonly Action _down;
    private bool _disposed;

    public DisposableWrapper(Action down) {
        this._down = down;
    }

    public DisposableWrapper(Action? up, Action down) {
        this._down = down;

        up?.Invoke();
    }

    public void Dispose() {
        if (this._disposed) return;
        
        this._down();
        this._disposed = true;

        GC.SuppressFinalize(this);
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using Dalamud.Interface.Windowing;
using Serilog;
using XIVDeck.FFXIVPlugin.IoC;
using XIVDeck.FFXIVPlugin.UI.Windows;
using XIVDeck.FFXIVPlugin.Utils;

namespace XIVDeck.FFXIVPlugin.UI;

[Service(ServiceFlags.Singleton | ServiceFlags.AutoLoad)]
public class UIManager(ILifetimeScope scope, ILogger log) : IDisposable {
    private readonly WindowSystem _windowSystem = new("XIVDeck");

    private readonly Dictionary<Type, XIVDeckWindow> _windows = new();
    private readonly List<XIVDeckWindow> _pendingDestroy = [];

    public T GetOrCreate<T>() where T : XIVDeckWindow {
        if (this._windows.TryGetValue(typeof(T), out var existing)) {
            return (T) existing;
        }

        var window = scope.Resolve<T>();

        this._windows[typeof(T)] = window;
        this._windowSystem.AddWindow(window);

        log.Debug("Created and registered window {Type}", typeof(T));

        return window;
    }

    public T? GetWindow<T>() where T : XIVDeckWindow {
        return this._windows.TryGetValue(typeof(T), out var window) ? (T) window : null;
    }

    public T ShowOrFocus<T>() where T : XIVDeckWindow {
        var window = this.GetOrCreate<T>();

        window.IsOpen = true;
        window.BringToFront();

        return window;
    }

    public void Close(Window window) {
        window.IsOpen = false;

        if (window is XIVDeckWindow managed && this._windows.ContainsKey(managed.GetType())) {
            this.QueueDestroy(managed);
        }
    }


    public void CloseAll<T>() where T : Window {
        foreach (var window in this._windows.Values.OfType<T>().Cast<Window>().ToList()) {
            this.Close(window);
        }
    }

    public void CloseAll() {
        this.CloseAll<Window>();
    }

    public void RequestTeardown(Window window) {
        if (window is XIVDeckWindow managed) this.QueueDestroy(managed);
    }

    private void QueueDestroy(XIVDeckWindow managed) {
        managed.IsOpen = false;

        if (!this._pendingDestroy.Contains(managed)) {
            this._pendingDestroy.Add(managed);
        }
    }

    private void ProcessPendingDestroys() {
        if (this._pendingDestroy.Count == 0) return;

        foreach (var managed in this._pendingDestroy) {
            if (!this._windows.Remove(managed.GetType())) continue;
            this._windowSystem.RemoveWindow(managed);

            log.Debug("Closed and unregistered window {Type}", managed.GetType());
        }

        this._pendingDestroy.Clear();
    }

    public void Dispose() {
        this.CloseAll();
        this.ProcessPendingDestroys();
        GC.SuppressFinalize(this);
    }

    public void Draw() {
        this._windowSystem.Draw();
        this.ProcessPendingDestroys();
    }
}

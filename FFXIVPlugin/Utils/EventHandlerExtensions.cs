using System;

namespace XIVDeck.FFXIVPlugin.Utils;

public static class EventHandlerExtensions {
    public static void InvokeSafely(this EventHandler? eh, object sender, EventArgs eventArgs, Action<Exception>? errorAction = null) {
        foreach (var handler in Delegate.EnumerateInvocationList(eh)) {
            try {
                handler(sender, eventArgs);
            } catch (Exception ex) {
                errorAction?.Invoke(ex);
            }
        }
    }

    public static void InvokeSafely<T>(this EventHandler<T>? eh, object sender, T eventArgs, Action<Exception>? errorAction = null) {
        foreach (var handler in Delegate.EnumerateInvocationList(eh)) {
            try {
                handler(sender, eventArgs);
            } catch (Exception ex) {
                errorAction?.Invoke(ex);
            }
        }
    }
}

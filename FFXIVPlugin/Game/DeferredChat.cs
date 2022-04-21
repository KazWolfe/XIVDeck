using System.Collections.Generic;
using Dalamud.Game.Text.SeStringHandling;
using XIVDeck.FFXIVPlugin.Base;

namespace XIVDeck.FFXIVPlugin.Game; 

public static class DeferredChat {
    private static readonly List<SeString> DeferredMessages = new();
    private static TickScheduler? _deferredTask;

    public static void SendOrDeferMessage(SeString message) {
        if (!Injections.ClientState.IsLoggedIn) {
            DeferredMessages.Add(message);
            return;
        }
        
        Injections.Chat.Print(message);
    }
    public static void SendDeferredMessages(long millis = 0) {
        // cancel a pre-existing deferred task if one exists - it seems reasonable that only one deferred send can
        // exist at once.
        _deferredTask?.Dispose();
        
        _deferredTask = TickScheduler.Schedule(() => {
            foreach (var message in DeferredMessages) {
                Injections.Chat.Print(message);
            }
        }, delay: millis);
    }

    public static void Cancel() {
        DeferredMessages.Clear();
        _deferredTask?.Dispose();
    } 
}
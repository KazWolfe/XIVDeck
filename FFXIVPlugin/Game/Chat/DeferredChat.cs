using System;
using System.Collections.Generic;
using System.Threading;
using Dalamud.Game.Text.SeStringHandling;
using XIVDeck.FFXIVPlugin.Base;

namespace XIVDeck.FFXIVPlugin.Game.Chat; 

internal static class DeferredChat {
    private static readonly List<SeString> DeferredMessages = new();
    private static CancellationTokenSource? _cts;

    internal static void SendOrDeferMessage(SeString message) {
        if (!Injections.ClientState.IsLoggedIn) {
            DeferredMessages.Add(message);
            return;
        }
        
        Injections.Chat.Print(message);
    }
    
    internal static void SendDeferredMessages(long millis = 0) {
        // Create a new CTS that can be used to cancel the next deferred message send 
        _cts = new CancellationTokenSource();
        
        Injections.Framework.RunOnTick(() => {
            DeferredMessages.ForEach(m => Injections.Chat.Print(m));
            DeferredMessages.Clear();
        }, delay: TimeSpan.FromMilliseconds(millis), cancellationToken: _cts.Token);
    }

    internal static void Cancel() {
        DeferredMessages.Clear();
        _cts?.Cancel();
    } 
}
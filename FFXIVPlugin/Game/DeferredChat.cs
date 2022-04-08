using System.Collections.Generic;
using Dalamud.Game.Text.SeStringHandling;
using XIVDeck.FFXIVPlugin.Base;

namespace XIVDeck.FFXIVPlugin.Game; 

public static class DeferredChat {
    private static readonly List<SeString> DeferredMessages = new();

    public static void SendOrDeferMessage(SeString message) {
        if (!Injections.ClientState.IsLoggedIn) {
            DeferredMessages.Add(message);
            return;
        }
        
        Injections.Chat.Print(message);
    }
    public static void SendDeferredMessages(long millis = 0) {
        TickScheduler.Schedule(() => {
            foreach (var message in DeferredMessages) {
                Injections.Chat.Print(message);
            }
        }, delay: millis);
    }
}
using System;
using XIVDeck.FFXIVPlugin.Game.Chat;
using XIVDeck.FFXIVPlugin.Server;
using XIVDeck.FFXIVPlugin.Server.Messages.Outbound;

namespace XIVDeck.FFXIVPlugin.Game; 

public static class DalamudHooks {
    public static void OnGameLogin(object? obj, EventArgs eventArgs) {
        // game state isn't ready until login succeeds, so we wait for it to be ready before updating cache
        XIVDeckPlugin.Instance.GameStateCache.Refresh();
        
        DeferredChat.SendDeferredMessages(6000);
        
        // this should probably be moved, but update macro/gearset states at login
        XIVDeckWSServer.Instance?.BroadcastMessage(new WSStateUpdateMessage("GearSet"));
        XIVDeckWSServer.Instance?.BroadcastMessage(new WSStateUpdateMessage("Macro"));
    }
}
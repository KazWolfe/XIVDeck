using Dalamud.Game.ClientState.Keys;
using XIVDeck.FFXIVPlugin.Utils;

namespace XIVDeck.FFXIVPlugin.Game; 

public static class GameUtils {
    internal static void ResetAFKTimer() {
        if (!InputUtil.TryFindGameWindow(out var windowHandle)) return;
        
        InputUtil.SendKeycode(windowHandle, (int) VirtualKey.RWIN);
    }
}
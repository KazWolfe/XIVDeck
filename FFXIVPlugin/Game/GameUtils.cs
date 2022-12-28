using System;
using XIVDeck.FFXIVPlugin.Utils;

namespace XIVDeck.FFXIVPlugin.Game; 

public static class GameUtils {
    // borrowed base logic from ChatTwo by ascclemens
    public static void SendSanitizedChatMessage(string text, bool commandOnly = true) {
        var plugin = XIVDeckPlugin.Instance;
            
        if (commandOnly && !text.StartsWith("/")) {
            throw new ArgumentException("The specified message message does not start with a slash while in command-only mode.");
        }
            
        // sanitization rules
        text = text.Replace("\n", " ");
        text = plugin.SigHelper.GetSanitizedString(text);
            
        plugin.SigHelper.SendChatMessage(text);
    }

    public static void ResetAFKTimer() {
        if (!InputUtil.TryFindGameWindow(out var windowHandle)) return;
        
        // Virtual key for Right Winkey. Can't be used by FFXIV normally, and in tests did not seem to cause any
        // unusual interference.
        InputUtil.SendKeycode(windowHandle, 0x5C);
    }
}
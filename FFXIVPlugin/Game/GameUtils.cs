using System;

namespace XIVDeck.FFXIVPlugin.Game; 

public static class GameUtils {
    public static bool IsCrossHotbar(int hotbarId) {
        return hotbarId switch {
            < 0 or > 19 => throw new ArgumentOutOfRangeException(nameof(hotbarId)),
            18 => false, // Standard pet/extra hotbar
            19 => true,  // Cross pet/extra hotbar
            _ => hotbarId >= 10
        };
    }
    
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
}
using System;

namespace XIVDeck.FFXIVPlugin.Utils; 

public static class GameUtils {
    public static bool IsCrossHotbar(int hotbarId) {
        return hotbarId switch {
            < 0 or > 19 => throw new ArgumentOutOfRangeException(nameof(hotbarId)),
            18 => false, // Standard pet/extra hotbar
            19 => true,  // Cross pet/extra hotbar
            _ => hotbarId >= 10
        };
    }
}
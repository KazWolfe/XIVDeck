using Dalamud.Game.ClientState.Keys;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using XIVDeck.FFXIVPlugin.Utils;

namespace XIVDeck.FFXIVPlugin.Game;

internal static class GameUtils {
    internal static unsafe void SendDummyInput() {
        var windowHandle = Framework.Instance()->GameWindow->WindowHandle;
        InputUtil.TapKey(windowHandle, (int) VirtualKey.RWIN); // winkey ignored by game normally.
    }
}

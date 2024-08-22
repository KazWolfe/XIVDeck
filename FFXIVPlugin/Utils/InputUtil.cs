using System.Runtime.InteropServices;

// ReSharper disable InconsistentNaming - meh.

namespace XIVDeck.FFXIVPlugin.Utils;

internal static partial class InputUtil {
    private const uint WM_KEYUP = 0x101;
    private const uint WM_KEYDOWN = 0x100;

    // ReSharper disable once UnusedMethodReturnValue.Local
    [LibraryImport("user32.dll")]
    private static partial nint SendMessageW(nint hWnd, uint msg, nint wParam, nint lParam);

    internal static void TapKey(nint windowHandle, int keycode) {
        SendMessageW(windowHandle, WM_KEYDOWN, keycode, 0);
        SendMessageW(windowHandle, WM_KEYUP, keycode, 0);
    }
}

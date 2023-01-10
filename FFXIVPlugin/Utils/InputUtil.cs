using System;
using System.Runtime.InteropServices;

// ReSharper disable InconsistentNaming - meh.

namespace XIVDeck.FFXIVPlugin.Utils; 

internal static partial class InputUtil {
    private const uint WM_KEYUP = 0x101;
    private const uint WM_KEYDOWN = 0x100;
    
    [LibraryImport("user32.dll", StringMarshalling = StringMarshalling.Utf16)]
    private static partial nint FindWindowExW(nint hWndParent, nint hWndChildAfter, string lpszClass, string? lpszWindow);

    [LibraryImport("user32.dll")]
    private static partial int GetWindowThreadProcessId(nint hWnd, out int processId);
    
    [LibraryImport("user32.dll")]
    private static partial nint SendMessageW(nint hWnd, uint msg, nint wParam, nint lParam);
    
    public static bool TryFindGameWindow(out nint handle) {
        handle = nint.Zero;
        while (true)
        {
            handle = FindWindowExW(nint.Zero, handle, "FFXIVGAME", null);
            if (handle == nint.Zero) break;
            var _ = GetWindowThreadProcessId(handle, out var pid);
            if (pid == Environment.ProcessId) break;
        }
        return handle != nint.Zero;
    }
    
    public static void SendKeycode(nint windowHandle, int keycode) {
        SendMessageW(windowHandle, WM_KEYDOWN, keycode, 0);
        SendMessageW(windowHandle, WM_KEYUP, keycode, 0);
    }
}
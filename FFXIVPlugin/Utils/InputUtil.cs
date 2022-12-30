using System;
using System.Runtime.InteropServices;

// ReSharper disable InconsistentNaming - meh.

namespace XIVDeck.FFXIVPlugin.Utils; 

internal static class InputUtil {
    private const uint WM_KEYUP = 0x101;
    private const uint WM_KEYDOWN = 0x100;
    
    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern IntPtr FindWindowEx(IntPtr hWndParent, IntPtr hWndChildAfter, string lpszClass, string? lpszWindow);

    [DllImport("user32.dll")]
    private static extern int GetWindowThreadProcessId(IntPtr hWnd, out int processId);
    
    [DllImport("user32.dll")]
    private static extern IntPtr SendMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);
    
    public static bool TryFindGameWindow(out IntPtr handle) {
        handle = IntPtr.Zero;
        while (true)
        {
            handle = FindWindowEx(IntPtr.Zero, handle, "FFXIVGAME", null);
            if (handle == IntPtr.Zero) break;
            var _ = GetWindowThreadProcessId(handle, out var pid);
            if (pid == Environment.ProcessId) break;
        }
        return handle != IntPtr.Zero;
    }
    
    public static void SendKeycode(IntPtr windowHandle, int keycode) {
        SendMessage(windowHandle, WM_KEYDOWN, (IntPtr)keycode, (IntPtr)0);
        SendMessage(windowHandle, WM_KEYUP, (IntPtr)keycode, (IntPtr)0);
    }
}
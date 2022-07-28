using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

// ReSharper disable InconsistentNaming - meh.

namespace XIVDeck.FFXIVPlugin.Utils; 

internal static class InputUtil {
    private const uint WM_KEYUP = 0x101;
    private const uint WM_KEYDOWN = 0x100;
    
    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern IntPtr FindWindowEx(IntPtr hWndParent, IntPtr hWndChildAfter, string lpszClass, string? lpszWindow);

    [DllImport("user32.dll")]
    private static extern int GetWindowThreadProcessId(IntPtr hWnd, out int lpdwProcessId);
    
    [DllImport("user32.dll")]
    private static extern IntPtr SendMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);
    
    public static bool TryFindGameWindow(out IntPtr hwnd) {
        hwnd = IntPtr.Zero;
        while (true)
        {
            hwnd = FindWindowEx(IntPtr.Zero, hwnd, "FFXIVGAME", null);
            if (hwnd == IntPtr.Zero) break;
            GetWindowThreadProcessId(hwnd, out var pid);
            if (pid == Process.GetCurrentProcess().Id) break;
        }
        return hwnd != IntPtr.Zero;
    }
    
    public static void SendKeycode(IntPtr hwnd, int keycode) {
        SendMessage(hwnd, WM_KEYDOWN, (IntPtr)keycode, (IntPtr)0);
        SendMessage(hwnd, WM_KEYUP, (IntPtr)keycode, (IntPtr)0);
    }
}
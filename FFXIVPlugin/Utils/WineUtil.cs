using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace XIVDeck.FFXIVPlugin.Utils;

public static class WineUtil {
    // Wine-only exports; these throw EntryPointNotFoundException on real Windows.
    [DllImport("kernel32.dll", EntryPoint = "wine_get_dos_file_name", CharSet = CharSet.Ansi, ExactSpelling = true)]
    private static extern IntPtr WineGetDosFileName(string unixFileName);

    [DllImport("kernel32.dll", EntryPoint = "wine_get_unix_file_name", CharSet = CharSet.Unicode, ExactSpelling = true)]
    private static extern IntPtr WineGetUnixFileName(string dosFileName);

    [DllImport("kernel32.dll", ExactSpelling = true)]
    private static extern IntPtr GetProcessHeap();

    [DllImport("kernel32.dll", ExactSpelling = true)]
    private static extern bool HeapFree(IntPtr hHeap, uint dwFlags, IntPtr lpMem);

    /// <summary>
    /// Gets the path to the user's preferred TMP in Wine format.
    /// </summary>
    /// <returns>
    /// A Windows path to the user's preferred temp. Null if not found or the user doesn't expose tmp (flatpak?).
    /// </returns>
    public static string? GetSharedTempDir() {
        var hostPath = GetHostSharedTempDir();

        return UnixToWinePath(hostPath);
    }

    /// <summary>
    /// Resolves the preferred temp dir on the Unix side in the priority:
    /// - WINE_HOST_XDG_RUNTIME_DIR/XDG_RUNTIME_DIR (observed on most Linux systems)
    ///   - If flatpak, grab the current context's Flatpak ID.
    /// - WINE_HOST_TMPDIR (observed on macOS and some Linux systems)
    /// - Just plain /tmp
    /// </summary>
    /// <returns></returns>
    private static string GetHostSharedTempDir() {
        var runtimeDir = Environment.GetEnvironmentVariable("WINE_HOST_XDG_RUNTIME_DIR")
                          ?? Environment.GetEnvironmentVariable("XDG_RUNTIME_DIR");

        if (!string.IsNullOrEmpty(runtimeDir)) {
            var flatpakId = Environment.GetEnvironmentVariable("FLATPAK_ID");
            if (!string.IsNullOrEmpty(flatpakId)) {
                return Path.Combine(runtimeDir, "app", flatpakId);
            }

            return runtimeDir;
        }

        var hostTmpDir = Environment.GetEnvironmentVariable("WINE_HOST_TMPDIR");
        return string.IsNullOrEmpty(hostTmpDir) ? "/tmp" : hostTmpDir;
    }

    /// <summary>
    /// Convert a Unix path to its Wine equivalent.
    /// </summary>
    /// <param name="unixPath">The path to convert.</param>
    /// <returns>The path, or null if it can't.</returns>
    public static string? UnixToWinePath(string unixPath) {
        return ConvertPath(() => WineGetDosFileName(unixPath), Marshal.PtrToStringUni);
    }

    /// <summary>
    /// Convert a Wine path to its Unix equivalent.
    /// </summary>
    /// <param name="winePath">The path to convert.</param>
    /// <returns>The path, or null if it can't (???).</returns>
    public static string? WineToUnixPath(string winePath) {
        return ConvertPath(() => WineGetUnixFileName(winePath), Marshal.PtrToStringAnsi);
    }

    private static string? ConvertPath(Func<IntPtr> convert, Func<IntPtr, string?> marshalResult) {
        IntPtr resultPtr;
        try {
            resultPtr = convert();
        } catch (EntryPointNotFoundException) {
            return null;
        }

        if (resultPtr == IntPtr.Zero) {
            return null;
        }

        try {
            return marshalResult(resultPtr);
        } finally {
            HeapFree(GetProcessHeap(), 0, resultPtr);
        }
    }

    /// <summary>
    /// Attempt to run a Unix command via Wine's `start.exe`. Used to escape the Wine sandbox if necessary.
    /// </summary>
    /// <param name="unixExecutablePath">The full path to execute.</param>
    /// <param name="args">An array of args to pass.</param>
    /// <returns>True if the command ran, false if not. Does not return info on status code.</returns>
    public static bool RunNativeCommand(string unixExecutablePath, params string[] args) {
        try {
            var startExe = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "start.exe");

            var psi = new ProcessStartInfo {
                FileName = startExe,
                UseShellExecute = true,
            };

            psi.ArgumentList.Add("/wait");
            psi.ArgumentList.Add("/unix");
            psi.ArgumentList.Add(unixExecutablePath);
            foreach (var arg in args) {
                psi.ArgumentList.Add(arg);
            }

            using var process = Process.Start(psi);
            return process != null && process.WaitForExit(TimeSpan.FromSeconds(5));
        } catch {
            return false;
        }
    }
}

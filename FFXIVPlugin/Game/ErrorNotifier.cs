using System;
using System.Collections.Generic;
using Dalamud.Logging;
using XIVDeck.FFXIVPlugin.Base;

namespace XIVDeck.FFXIVPlugin.Game; 

public static class ErrorNotifier {
    private const int DebounceTime = 300;
    private static readonly Dictionary<string, long> Debounce = new();

    public static void ShowError(string text, bool useToast = false) {
        if (Debounce.GetValueOrDefault(text, 0) > Environment.TickCount64) {
            PluginLog.Verbose("Debounce triggered for error");
            return;
        }
        
        Injections.Chat.PrintError(text);

        if (useToast) {
            Injections.Toasts.ShowError(text);
        }

        Debounce[text] = Environment.TickCount64 + DebounceTime;
    }
}
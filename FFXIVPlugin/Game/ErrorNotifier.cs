using System;
using System.Collections.Generic;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Logging;
using XIVDeck.FFXIVPlugin.Base;
using XIVDeck.FFXIVPlugin.Resources.Localization;

namespace XIVDeck.FFXIVPlugin.Game; 

public static class ErrorNotifier {
    private const int DebounceTime = 300;
    private static readonly Dictionary<string, long> Debounce = new();

    public static SeString BuildPrefixedString(SeString message, int colorKey = 514) {
        return new SeStringBuilder()
            .AddUiForeground($"[{UIStrings.XIVDeck}] ", (ushort) colorKey)
            .Append(message)
            .Build();
    }

    public static void ShowError(string text, bool useToast = false, bool prefix = true, bool debounce = false) {
        if (debounce && Debounce.GetValueOrDefault(text, 0) > Environment.TickCount64) {
            PluginLog.Verbose($"ShowError fired but suppressed by debounce: {text}");
            return;
        }
        
        Injections.Chat.PrintError(prefix ? BuildPrefixedString(text) : text);

        if (useToast) 
            Injections.Toasts.ShowError(text);

        if (debounce) 
            Debounce[text] = Environment.TickCount64 + DebounceTime;
    }
}
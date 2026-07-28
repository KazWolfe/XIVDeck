using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Plugin.Services;
using Serilog;
using XIVDeck.FFXIVPlugin.IoC;
using XIVDeck.FFXIVPlugin.Resources.Localization;

namespace XIVDeck.FFXIVPlugin.Game.Chat;

[Service(ServiceFlags.Singleton)]
public class ErrorNotifier(ILogger log, IToastGui toasts, IChatGui chat) {
    private const int DebounceTime = 300;
    private readonly ConcurrentDictionary<string, long> _debounce = new();

    public static SeString BuildPrefixedString(SeString message, int colorKey = 514) {
        return new SeStringBuilder()
            .AddUiForeground($"[{UIStrings.XIVDeck}] ", (ushort) colorKey)
            .Append(message)
            .Build();
    }

    public void ShowError(string text, bool useToast = false, bool prefix = true, bool debounce = false) {
        if (debounce && this._debounce.GetValueOrDefault(text, 0) > Environment.TickCount64) {
            log.Verbose("ShowError fired but suppressed by debounce: {Text}", text);
            return;
        }

        chat.PrintError(prefix ? BuildPrefixedString(text) : text);

        if (useToast)
            toasts.ShowError(text);

        if (debounce) this._debounce[text] = Environment.TickCount64 + DebounceTime;
    }
}

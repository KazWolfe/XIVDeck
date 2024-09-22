using System;
using EmbedIO;
using Swan.Logging;
using XIVDeck.FFXIVPlugin.Base;
using XIVDeck.FFXIVPlugin.Exceptions;
using XIVDeck.FFXIVPlugin.Utils;

namespace XIVDeck.FFXIVPlugin.Server.Helpers;

public class PluginLogShim : ILogger {
    public LogLevel LogLevel { get; }

    public PluginLogShim(LogLevel level = LogLevel.Warning) {
        this.LogLevel = level;
    }

    public void Dispose() {
        GC.SuppressFinalize(this);
    }

    public void Log(LogMessageReceivedEventArgs logEvent) {
        // Ignore HTTP exceptions, as they're probably going to be handled elsewhere.
        if (logEvent.Exception is HttpException or IXIVDeckException) return;

        Injections.PluginLog.Write(logEvent.MessageType.AsSerilogLevel(), logEvent.Exception,
            $"[EmbedIO] {logEvent.Message}");
    }
}

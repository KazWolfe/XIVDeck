using System;
using Dalamud.Logging;
using EmbedIO;
using Swan.Logging;
using XIVDeck.FFXIVPlugin.Exceptions;

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

        switch (logEvent.MessageType) {
            case LogLevel.Trace:
                PluginLog.Debug(logEvent.Exception!, $"[EmbedIO] {logEvent.Message}");
                break;
            case LogLevel.Debug:
                PluginLog.Debug(logEvent.Exception!, $"[EmbedIO] {logEvent.Message}");
                break;
            case LogLevel.Info:
                PluginLog.Debug(logEvent.Exception!, $"[EmbedIO] {logEvent.Message}");
                break;
            case LogLevel.Warning:
                PluginLog.Warning(logEvent.Exception!, $"[EmbedIO] {logEvent.Message}");
                break;
            case LogLevel.Error:
                PluginLog.Error(logEvent.Exception!, $"[EmbedIO] {logEvent.Message}");
                break;
            case LogLevel.Fatal:
                PluginLog.Fatal(logEvent.Exception!, $"[EmbedIO] {logEvent.Message}");
                break;
            case LogLevel.None:
            default:
                PluginLog.Information(logEvent.Exception!, $"[EmbedIO] {logEvent.Message}");
                break;
        }
    }
}
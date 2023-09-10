using System;
using EmbedIO;
using Swan.Logging;
using XIVDeck.FFXIVPlugin.Base;
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
                Injections.PluginLog.Debug(logEvent.Exception!, $"[EmbedIO] {logEvent.Message}");
                break;
            case LogLevel.Debug:
                Injections.PluginLog.Debug(logEvent.Exception!, $"[EmbedIO] {logEvent.Message}");
                break;
            case LogLevel.Info:
                Injections.PluginLog.Debug(logEvent.Exception!, $"[EmbedIO] {logEvent.Message}");
                break;
            case LogLevel.Warning:
                Injections.PluginLog.Warning(logEvent.Exception!, $"[EmbedIO] {logEvent.Message}");
                break;
            case LogLevel.Error:
                Injections.PluginLog.Error(logEvent.Exception!, $"[EmbedIO] {logEvent.Message}");
                break;
            case LogLevel.Fatal:
                Injections.PluginLog.Fatal(logEvent.Exception!, $"[EmbedIO] {logEvent.Message}");
                break;
            case LogLevel.None:
            default:
                Injections.PluginLog.Information(logEvent.Exception!, $"[EmbedIO] {logEvent.Message}");
                break;
        }
    }
}
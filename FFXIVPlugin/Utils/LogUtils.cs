namespace XIVDeck.FFXIVPlugin.Utils;

public static class LogUtils {
    public static Serilog.Events.LogEventLevel AsSerilogLevel(this Swan.Logging.LogLevel level) {
        return level switch {
            Swan.Logging.LogLevel.Trace => Serilog.Events.LogEventLevel.Verbose,
            Swan.Logging.LogLevel.Debug => Serilog.Events.LogEventLevel.Debug,
            Swan.Logging.LogLevel.Info => Serilog.Events.LogEventLevel.Information,
            Swan.Logging.LogLevel.Warning => Serilog.Events.LogEventLevel.Warning,
            Swan.Logging.LogLevel.Error => Serilog.Events.LogEventLevel.Error,
            Swan.Logging.LogLevel.Fatal => Serilog.Events.LogEventLevel.Fatal,
            _ => Serilog.Events.LogEventLevel.Information
        };
    }
}

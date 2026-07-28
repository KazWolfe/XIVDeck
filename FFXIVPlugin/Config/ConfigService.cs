using System;
using System.IO;
using Dalamud.Interface.ImGuiNotification;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;
using XIVDeck.FFXIVPlugin.IoC;
using XIVDeck.FFXIVPlugin.Resources.Localization;

namespace XIVDeck.FFXIVPlugin.Config;


[Service(ServiceFlags.Singleton)]
public class ConfigService {
    public const int CurrentVersion = 1;

    private readonly IDalamudPluginInterface _pluginInterface;
    private readonly ILogger _log;
    private readonly INotificationManager _notifications;

    public PluginConfig Config { get; }

    public ConfigService(IDalamudPluginInterface pluginInterface, ILogger log, INotificationManager notifications) {
        this._pluginInterface = pluginInterface;
        this._log = log;
        this._notifications = notifications;

        try {
            this.Config = this.Load();
        } catch (Exception ex) {
            this.Config = this.ResetConfig(ex);
        }
    }

    public void Save() {
        this._pluginInterface.SavePluginConfig(this.Config);
    }

    private PluginConfig Load() {
        var file = this._pluginInterface.ConfigFile;
        if (!file.Exists) return new PluginConfig();

        var json = File.ReadAllText(file.FullName);
        var version = JObject.Parse(json)["Version"]?.Value<int>() ?? 0;

        if (version >= CurrentVersion) {
            // If we're already current or newer, just try loading as-is.
            return JsonConvert.DeserializeObject<PluginConfig>(json) ?? new PluginConfig();
        }

        if (!ConfigMigrations.VersionTypes.TryGetValue(version, out var historicalType)) {
            throw new JsonException($"Don't know how to load plugin config version {version}.");
        }

        var config = JsonConvert.DeserializeObject(json, historicalType)!;
        var previousJson = json;

        for (var v = version; v < CurrentVersion; v++) {
            config = ConfigMigrations.Apply(v, config);
            var migratedJson = JsonConvert.SerializeObject(config);

            this._log.Information("Migrated plugin config from version {From} to {To}.\nOld: {OldConfig}\nNew: {NewConfig}",
                v, v + 1, previousJson, migratedJson);
            previousJson = migratedJson;
        }

        var migrated = (PluginConfig) config;

        this._pluginInterface.SavePluginConfig(migrated);

        return migrated;
    }

    private PluginConfig ResetConfig(Exception loadException) {
        this._log.Error(loadException, "Plugin config {Path} could not be loaded and has been reset to defaults.",
            this._pluginInterface.ConfigFile.FullName);

        var config = new PluginConfig();
        this._pluginInterface.SavePluginConfig(config);

        this._notifications.AddNotification(new Notification {
            Title = UIStrings.ConfigService_ConfigReset_Title,
            Content = UIStrings.ConfigService_ConfigReset_Description,
            Type = NotificationType.Error,
            InitialDuration = TimeSpan.MaxValue,
        });

        return config;
    }
}

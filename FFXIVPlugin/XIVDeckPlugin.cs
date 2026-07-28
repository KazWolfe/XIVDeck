using System.Globalization;
using System.Linq;
using System.Reflection;
using Autofac;
using Autofac.Core;
using Autofac.Core.Resolving.Pipeline;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using JetBrains.Annotations;
using Serilog;
using XIVDeck.FFXIVPlugin.Config;
using XIVDeck.FFXIVPlugin.IoC;
using XIVDeck.FFXIVPlugin.Resources.Localization;
using XIVDeck.FFXIVPlugin.RpcServer;
using XIVDeck.FFXIVPlugin.RpcServer.Services;
using XIVDeck.FFXIVPlugin.RpcServer.Transports;
using XIVDeck.FFXIVPlugin.UI;
using XIVDeck.FFXIVPlugin.UI.Windows;
using XIVDeck.FFXIVPlugin.UI.Windows.Nags;
using XIVDeck.FFXIVPlugin.Utils;

namespace XIVDeck.FFXIVPlugin;

[UsedImplicitly]
public sealed class XIVDeckPlugin : IDalamudPlugin {
    public string Name => UIStrings.XIVDeckPlugin_Name;

    private ConfigService ConfigService { get; }
    private UIManager UIManager { get; }
    private IDalamudPluginInterface PluginInterface { get; }
    private IContainer PluginContainer { get; }

    public XIVDeckPlugin(IDalamudPluginInterface pluginInterface) {
        this.PluginInterface = pluginInterface;

        var builder = new ContainerBuilder();

        builder.RegisterInstance(pluginInterface).As<IDalamudPluginInterface>().ExternallyOwned();

        // Dalamud service injection
        builder.RegisterInstance(pluginInterface.GetService<IChatGui>()).As<IChatGui>().ExternallyOwned();
        builder.RegisterInstance(pluginInterface.GetService<IClientState>()).As<IClientState>().ExternallyOwned();
        builder.RegisterInstance(pluginInterface.GetService<ICondition>()).As<ICondition>().ExternallyOwned();
        builder.RegisterInstance(pluginInterface.GetService<IDataManager>()).As<IDataManager>().ExternallyOwned();
        builder.RegisterInstance(pluginInterface.GetService<IFramework>()).As<IFramework>().ExternallyOwned();
        builder.RegisterInstance(pluginInterface.GetService<IGameGui>()).As<IGameGui>().ExternallyOwned();
        builder.RegisterInstance(pluginInterface.GetService<IGameConfig>()).As<IGameConfig>().ExternallyOwned();
        builder.RegisterInstance(pluginInterface.GetService<IGameInteropProvider>()).As<IGameInteropProvider>().ExternallyOwned();
        builder.RegisterInstance(pluginInterface.GetService<INotificationManager>()).As<INotificationManager>().ExternallyOwned();
        builder.RegisterInstance(pluginInterface.GetService<ITextureSubstitutionProvider>()).As<ITextureSubstitutionProvider>().ExternallyOwned();
        builder.RegisterInstance(pluginInterface.GetService<IToastGui>()).As<IToastGui>().ExternallyOwned();
        builder.RegisterInstance(pluginInterface.GetService<IUnlockState>()).As<IUnlockState>().ExternallyOwned();

        var pluginLog = pluginInterface.GetService<IPluginLog>();
        builder.RegisterInstance(pluginLog.Logger).As<ILogger>().ExternallyOwned();

        builder.ComponentRegistryBuilder.Registered += (_, registeredArgs) => {
            // Inject a per-type logger as appropriate.
            registeredArgs.ComponentRegistration.PipelineBuilding += (_, pipeline) => {
                pipeline.Use(PipelinePhase.ParameterSelection, (ctxt, next) => {
                    ctxt.ChangeParameters(ctxt.Parameters.Append(new ResolvedParameter(
                        (pi, _) => pi.ParameterType == typeof(ILogger),
                        (pi, c) => c.Resolve<ILogger>().ForContext(pi.Member.DeclaringType ?? typeof(XIVDeckPlugin)))));

                    next(ctxt);
                });
            };
        };

        builder.RegisterAssemblyTypes(typeof(XIVDeckPlugin).Assembly)
            .Where(type => type.GetCustomAttribute<ServiceAttribute>() is { } attr && !attr.Flags.HasFlag(ServiceFlags.Transient))
            .AsSelf()
            .SingleInstance();

        builder.RegisterAssemblyTypes(typeof(XIVDeckPlugin).Assembly)
            .Where(type => type.GetCustomAttribute<ServiceAttribute>() is { } attr && attr.Flags.HasFlag(ServiceFlags.Transient))
            .AsSelf();

        builder.RegisterType<UnixSocketServer>().Keyed<IRpcTransport>(TransportType.UnixDomainSocket).SingleInstance();
        builder.RegisterType<NamedPipeServer>().Keyed<IRpcTransport>(TransportType.NamedPipe).SingleInstance();
        builder.RegisterType<WebSocketServer>().Keyed<IRpcTransport>(TransportType.WebSocket).SingleInstance();

        builder.Register(c => c.Resolve<ConfigService>().Config).As<PluginConfig>().SingleInstance();

        this.PluginContainer = builder.Build();

        this.ConfigService = this.PluginContainer.Resolve<ConfigService>();
        this.UIManager = this.PluginContainer.Resolve<UIManager>();

        foreach (var serviceType in typeof(XIVDeckPlugin).Assembly.GetTypes()) {
            if (serviceType.GetCustomAttribute<ServiceAttribute>()?.Flags.HasFlag(ServiceFlags.AutoLoad) == true) {
                this.PluginContainer.Resolve(serviceType);
            }
        }

        this.PluginInterface.UiBuilder.Draw += this.UIManager.Draw;
        this.PluginInterface.UiBuilder.OpenConfigUi += this.DrawConfigUI;

        this.PluginInterface.LanguageChanged += this.UpdateLang;
        this.UpdateLang(this.PluginInterface.UiLanguage);

        this.InitializeNag();
    }

    public void Dispose() {
        this.PluginInterface.LanguageChanged -= this.UpdateLang;
        this.PluginInterface.UiBuilder.OpenConfigUi -= this.DrawConfigUI;
        this.PluginInterface.UiBuilder.Draw -= this.UIManager.Draw;

        this.PluginContainer.Dispose();
    }

    internal void DrawConfigUI() {
        this.UIManager.GetOrCreate<SettingsWindow>().Toggle();
    }

    private void InitializeNag() {
        if (!this.ConfigService.Config.HasLinkedStreamDeckPlugin) {
            this.UIManager.ShowOrFocus<SetupNag>();
        }
    }

    private void UpdateLang(string langCode) {
        UIStrings.Culture = new CultureInfo(langCode);
    }
}

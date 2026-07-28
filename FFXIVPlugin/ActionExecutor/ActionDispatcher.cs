using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Autofac;
using Autofac.Features.Metadata;
using JetBrains.Annotations;
using Serilog;
using XIVDeck.FFXIVPlugin.IoC;
using static FFXIVClientStructs.FFXIV.Client.UI.Misc.RaptureHotbarModule;

namespace XIVDeck.FFXIVPlugin.ActionExecutor;

[Service(ServiceFlags.Singleton)]
public class ActionDispatcher : IDisposable {
    private Dictionary<HotbarSlotType, IActionStrategy> Strategies { get; } = new();
    private ILifetimeScope StrategyScope { get; }

    public ActionDispatcher(ILogger log, ILifetimeScope scope) {
        this.StrategyScope = scope.BeginLifetimeScope(builder => {
            builder.RegisterAssemblyTypes(typeof(ActionDispatcher).Assembly)
                .Where(type => typeof(IActionStrategy).IsAssignableFrom(type) &&
                               type.GetCustomAttribute<ActionStrategyAttribute>() != null)
                .As<IActionStrategy>()
                .WithMetadata(ActionStrategyMetadataKeys.HotbarSlotType,
                    type => type.GetCustomAttribute<ActionStrategyAttribute>()!.HotbarSlotType);
        });

        foreach (var strategy in this.StrategyScope.Resolve<IEnumerable<Meta<IActionStrategy>>>()) {
            var slotType = (HotbarSlotType) strategy.Metadata[ActionStrategyMetadataKeys.HotbarSlotType]!;
            var handler = strategy.Value;

            // Hack to load everything (especially Lumina) synchronously to avoid issues
            try {
                handler.GetSelectableActions();
            } catch (Exception ex) {
                log.Warning(ex, "Could not populate strategy for {SlotType}!", slotType);
            }

            log.Debug("Registered strategy for {SlotType}: {StrategyType}", slotType, handler.GetType().Name);
            this.Strategies[slotType] = handler;
        }
    }

    public bool TryGetStrategyForType(HotbarSlotType type, [NotNullWhen(true)] out IActionStrategy? strategy) {
        return this.Strategies.TryGetValue(type, out strategy);
    }

    public ReadOnlyDictionary<HotbarSlotType, IActionStrategy> GetStrategies() {
        return this.Strategies.AsReadOnly();
    }

    public void Dispose() {
        this.StrategyScope.Dispose();
        GC.SuppressFinalize(this);
    }
}

[AttributeUsage(AttributeTargets.Class)]
[MeansImplicitUse]
public class ActionStrategyAttribute(HotbarSlotType hotbarSlotType) : Attribute {
    public readonly HotbarSlotType HotbarSlotType = hotbarSlotType;
}

internal static class ActionStrategyMetadataKeys {
    public const string HotbarSlotType = "HotbarSlotType";
}

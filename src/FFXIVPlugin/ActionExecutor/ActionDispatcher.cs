using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using Dalamud.Logging;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;

namespace XIVDeck.FFXIVPlugin.ActionExecutor; 

public class ActionDispatcher {
    private Dictionary<HotbarSlotType, IActionStrategy> Strategies { get; } = new();

    public ActionDispatcher() {
        foreach (var type in Assembly.GetExecutingAssembly().GetTypes()) {
            if (!type.GetInterfaces().Contains(typeof(IActionStrategy))) {
                continue;
            }

            var attr = type.GetCustomAttribute<ActionStrategyAttribute>();
            if (attr == null) continue;

            var slotType = attr.HotbarSlotType;

            var handler = Activator.CreateInstance(type) as IActionStrategy;

            if (handler == null) {
                PluginLog.Error($"Could not create strategy for {Enum.GetName(slotType)}!");
                continue;
            }

            // Hack to load everything (especially Lumina) synchronously to avoid issues
            try {
                handler.GetAllowedItems();
            } catch (Exception ex) {
                PluginLog.Warning(ex, $"Could not populate strategy for {Enum.GetName(slotType)}!");
            }
            
            PluginLog.Debug($"Registered strategy for {Enum.GetName(slotType)}: {handler.GetType()}");
            this.Strategies[slotType] = handler;
        }
    }

    public IActionStrategy GetStrategyForType(HotbarSlotType type) {
        return this.Strategies[type];
    }

    public ReadOnlyDictionary<HotbarSlotType, IActionStrategy> GetStrategies() {
        return this.Strategies.AsReadOnly();
    }
}

[AttributeUsage(AttributeTargets.Class)]
public class ActionStrategyAttribute : Attribute {
    public readonly HotbarSlotType HotbarSlotType;

    public ActionStrategyAttribute(HotbarSlotType hotbarSlotType) {
        this.HotbarSlotType = hotbarSlotType;
    }
}
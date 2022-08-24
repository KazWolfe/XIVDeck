using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Dalamud.Logging;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using XIVDeck.FFXIVPlugin.Base;
using XIVDeck.FFXIVPlugin.Exceptions;

namespace XIVDeck.FFXIVPlugin.ActionExecutor; 

public class ActionDispatcher {
        
    /* STRATEGIES
     *
     * Emote: {textCommand}
     * Mount: /mount {mountName}
     * Minion: /minion {minionName}
     * 
     * Gearset: /gs change {number} {glamour}
     * General Action: /ac {actionName}
     *
     * Marker: /marker {marker}
     * Waymark: /waymark {marker}
     *
     * Macro: custom - implementation for these is a bit weird as there's no "macro sheet"
     * 
     */
        
    private static readonly ActionDispatcher Instance = new();
        
    public static IActionStrategy GetStrategyForSlotType(HotbarSlotType type) {
        return Instance.GetStrategyForType(type);
    }

    public static List<HotbarSlotType> GetSupportedActions() {
        return Instance._GetSupportedActions();
    }

    public static Dictionary<HotbarSlotType, IActionStrategy> GetStrategies() {
        return Instance.Strategies;
    }
        
    public static void Execute(HotbarSlotType actionType, int actionId, IDictionary<string, dynamic>? options = null) {
        if (!Injections.ClientState.IsLoggedIn)
            throw new PlayerNotLoggedInException();
            
        GetStrategyForSlotType(actionType).Execute((uint) actionId, options);
    }

    private Dictionary<HotbarSlotType, IActionStrategy> Strategies { get; } = new();

    private ActionDispatcher() {
        // once again, autowiring. this is a net LoC loss, but looks cleaner than just manually populating the list
        // IMO. 
        foreach (var type in Assembly.GetExecutingAssembly().GetTypes()) {
            if (!type.GetInterfaces().Contains(typeof(IActionStrategy))) {
                continue;
            }

            var attr = type.GetCustomAttribute<ActionStrategyAttribute>();
            if (attr == null) continue;

            var slotType = attr.HotbarSlotType;

            var handler = (IActionStrategy) Activator.CreateInstance(type)!;

            // ToDo: Remove - this is a hack to force Lumina to bring everything it needs into memory.
            try {
                handler.GetAllowedItems();
            } catch (Exception ex) {
                PluginLog.Error(ex, $"Could not load strategy for {Enum.GetName(slotType)}!");
            }
            
            
            PluginLog.Debug($"Registered strategy for {Enum.GetName(slotType)}: {handler.GetType()}");
            this.Strategies[slotType] = handler;
        }
    }

    private IActionStrategy GetStrategyForType(HotbarSlotType type) {
        return this.Strategies[type];
    }

    private List<HotbarSlotType> _GetSupportedActions() {
        return this.Strategies.Keys.ToList();
    }
}

[AttributeUsage(AttributeTargets.Class)]
public class ActionStrategyAttribute : Attribute {
    public HotbarSlotType HotbarSlotType;

    public ActionStrategyAttribute(HotbarSlotType hotbarSlotType) {
        this.HotbarSlotType = hotbarSlotType;
    }
}
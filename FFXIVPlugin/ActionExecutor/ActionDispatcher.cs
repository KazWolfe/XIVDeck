using System.Collections.Generic;
using System.Linq;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using FFXIVPlugin.ActionExecutor.Strategies;

namespace FFXIVPlugin.ActionExecutor {
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
        
        
        public static ActionDispatcher Instance = new ActionDispatcher();
        
        public static IBaseStrategy GetStrategyForSlotType(HotbarSlotType type) {
            return Instance.GetStrategyForType(type);
        }

        public static List<HotbarSlotType> GetSupportedActions() {
            return Instance._GetSupportedActions();
        }

        public static Dictionary<HotbarSlotType, IBaseStrategy> GetStrategies() {
            return Instance.Strategies;
        }

        private Dictionary<HotbarSlotType, IBaseStrategy> Strategies { get; } = new();

        private ActionDispatcher() {
            this.Strategies[HotbarSlotType.Emote] = new EmoteStrategy();
            this.Strategies[HotbarSlotType.Minion] = new MinionStrategy();
            this.Strategies[HotbarSlotType.Mount] = new MountStrategy();
            this.Strategies[HotbarSlotType.Macro] = new MacroStrategy();
        }

        private IBaseStrategy GetStrategyForType(HotbarSlotType type) {
            return this.Strategies[type];
        }

        private List<HotbarSlotType> _GetSupportedActions() {
            return this.Strategies.Keys.ToList();
        }
    }
}
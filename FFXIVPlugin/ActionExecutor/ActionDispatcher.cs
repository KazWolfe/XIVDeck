using System.Collections.Generic;
using System.Linq;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using XIVDeck.FFXIVPlugin.ActionExecutor.Strategies;
using XIVDeck.FFXIVPlugin.Base;
using XIVDeck.FFXIVPlugin.Exceptions;

namespace XIVDeck.FFXIVPlugin.ActionExecutor {
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
            this.Strategies[HotbarSlotType.Emote] = new EmoteStrategy();
            this.Strategies[HotbarSlotType.Minion] = new MinionStrategy();
            this.Strategies[HotbarSlotType.Mount] = new MountStrategy();
            this.Strategies[HotbarSlotType.Macro] = new MacroStrategy();
            this.Strategies[HotbarSlotType.GeneralAction] = new GeneralActionStrategy();
            this.Strategies[HotbarSlotType.FieldMarker] = new WaymarkStrategy();
            this.Strategies[HotbarSlotType.Marker] = new MarkerStrategy();
            this.Strategies[HotbarSlotType.MainCommand] = new MainCommandStrategy();
            this.Strategies[HotbarSlotType.GearSet] = new GearsetStrategy();
            this.Strategies[HotbarSlotType.ExtraCommand] = new ExtraCommandStrategy();
            this.Strategies[HotbarSlotType.FashionAccessory] = new OrnamentStrategy();
            this.Strategies[HotbarSlotType.PerformanceInstrument] = new InstrumentStrategy();
            this.Strategies[HotbarSlotType.Collection] = new CollectionStrategy();
        }

        private IActionStrategy GetStrategyForType(HotbarSlotType type) {
            return this.Strategies[type];
        }

        private List<HotbarSlotType> _GetSupportedActions() {
            return this.Strategies.Keys.ToList();
        }
    }
}
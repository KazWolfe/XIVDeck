using System.Collections.Generic;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using NetCoreServer;
using Newtonsoft.Json;
using XIVDeck.FFXIVPlugin.ActionExecutor;
using XIVDeck.FFXIVPlugin.Server.Messages.Outbound;

namespace XIVDeck.FFXIVPlugin.Server.Messages.Inbound {
    public class WSGetUnlockedActionsOpcode : BaseInboundMessage {
        public override WSUnlockedActionsMessage Process(WsSession session) {
            // refresh the cache first so we have all the newest data available
            XIVDeckPlugin.Instance.GameStateCache.Refresh();
            
            Dictionary<HotbarSlotType, List<ExecutableAction>> actions = new();

            foreach (var (type, strategy) in ActionDispatcher.GetStrategies()) {
                var allowedItems = strategy.GetAllowedItems();
                if (allowedItems == null || allowedItems.Count == 0) continue;
                
                actions[type] = allowedItems;
            }

            return new WSUnlockedActionsMessage(actions);
        }
        
        public WSGetUnlockedActionsOpcode() : base("getUnlockedActions") { }

    }

    public class WSUnlockedActionsMessage : BaseOutboundMessage {
        // ToDo: Rename this field to `actions`.
        [JsonProperty("data")]
        public Dictionary<HotbarSlotType, List<ExecutableAction>> AllowedActions;

        public WSUnlockedActionsMessage(Dictionary<HotbarSlotType, List<ExecutableAction>> allowedActions) : base("unlockedActions") { 
            this.AllowedActions = allowedActions;
        }
    }
}
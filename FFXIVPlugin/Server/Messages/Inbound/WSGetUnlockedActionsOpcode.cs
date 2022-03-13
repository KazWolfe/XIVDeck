using System.Collections.Generic;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using NetCoreServer;
using Newtonsoft.Json;
using XIVDeck.FFXIVPlugin.ActionExecutor;

namespace XIVDeck.FFXIVPlugin.Server.Messages.Inbound {
    public class WSGetUnlockedActionsOpcode : BaseInboundMessage {
        public override void Process(WsSession session) {
            // refresh the cache first so we have all the newest data available
            XIVDeckPlugin.Instance.GameStateCache.Refresh();
            
            Dictionary<HotbarSlotType, List<ExecutableAction>> actions = new();

            foreach (var (type, strategy) in ActionDispatcher.GetStrategies()) {
                var allowedItems = strategy.GetAllowedItems();
                if (allowedItems == null || allowedItems.Count == 0) continue;
                
                actions[type] = allowedItems;
            }

            var reply = new Dictionary<string, dynamic> {
                ["messageType"] = "unlockedActions",
                ["data"] = actions
            };

            session.SendTextAsync(JsonConvert.SerializeObject(reply));
        }
        
        public WSGetUnlockedActionsOpcode() : base("getUnlockedActions") { }

    }
}
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using XIVDeck.FFXIVPlugin.Game;
using XIVDeck.FFXIVPlugin.Server.Types;

namespace XIVDeck.FFXIVPlugin.Server.Messages.Inbound {
    public class WSGetClassesOpcode : BaseInboundMessage {
        private readonly GameStateCache _gameStateCache = XIVDeckPlugin.Instance.GameStateCache;

        public override void Process(XIVDeckRoute session) {
            this._gameStateCache.Refresh();

            var availableClasses = this._gameStateCache.Gearsets!
                .Select(gearset => (int) gearset.ClassJob)
                .ToList();

            session.SendMessage(new WSClassesMessage(availableClasses));
        }

        public WSGetClassesOpcode() : base("getClasses") { }
    }

    public class WSClassesMessage : BaseOutboundMessage {
        [JsonProperty("classes")]
        public List<SerializableGameClass> Classes { get; }

        [JsonProperty("available")] public List<int> AvailableClasses { get; }

        public WSClassesMessage(List<int> availableClasses) : base("gameClasses") {
            this.Classes = SerializableGameClass.GetCache();
            this.AvailableClasses = availableClasses;
        }
    }
}
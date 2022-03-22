using System.Threading.Tasks;
using EmbedIO.WebSockets;
using Newtonsoft.Json;
using XIVDeck.FFXIVPlugin.Server.Messages;

namespace XIVDeck.FFXIVPlugin.Server {
    public class WSEventNotifier : WebSocketModule {
        public static WSEventNotifier? Instance { get; private set; }

        public WSEventNotifier(string urlPath) : base(urlPath, true) {
            Instance = this;
        }

        protected override Task OnMessageReceivedAsync(IWebSocketContext context, byte[] buffer, IWebSocketReceiveResult result) {
            // do nothing, WS only notifies the plugin and consumes no data
            return new Task(() => { });
        }

        public new void Dispose() {
            Instance = null;
            base.Dispose();
        }

        public void BroadcastString(string payload) {
            this.BroadcastAsync(payload);
        }

        public void BroadcastMessage(BaseOutboundMessage message) {
            var serializedData = JsonConvert.SerializeObject(message);
            
            this.BroadcastString(serializedData);
        }
    }
}; 


using System.Collections.Generic;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using FFXIVPlugin.helpers;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using FFXIVPlugin.Server.Messages.Inbound;
using FFXIVPlugin.Utils;
using Lumina.Data.Files;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace FFXIVPlugin.Server.Messages.Outbound {
    /**
     * A WebSocket message to disclose a generic update of state, generally for actions or some other case where it
     * would be a good idea for the Stream Deck plugin to refresh (a subset of) information.
     */
    public class WSStateUpdateMessage : BaseOutboundMessage {
        private static string MESSAGE_NAME = "stateUpdate";
        
        [JsonProperty("type")] public string StateType { get; set; }
        [JsonProperty("params")] public string Parameters { get; set; }

        public WSStateUpdateMessage() : base(MESSAGE_NAME) { }
    }
}
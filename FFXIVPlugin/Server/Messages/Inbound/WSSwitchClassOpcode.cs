using System;
using Dalamud.Logging;
using FFXIVPlugin.Base;
using FFXIVPlugin.Server.Messages.Outbound;
using FFXIVPlugin.Utils;
using Lumina.Excel.GeneratedSheets;
using NetCoreServer;
using Newtonsoft.Json;

namespace FFXIVPlugin.Server.Messages.Inbound {
    public class WSSwitchClassOpcode : BaseInboundMessage {
        private static GameStateCache _gameStateCache = XIVDeckPlugin.Instance.GameStateCache;
        
        [JsonProperty("id")] int Id { get; set; }

        public override void Process(WsSession session) {
            if (this.Id < 1)
                throw new ArgumentException( "Cannot switch to a class with ID less than 1");
            
            if (!Injections.ClientState.IsLoggedIn)
                throw new InvalidOperationException("A player is not logged in to the game!");
            
            _gameStateCache.Refresh();
            
            foreach (var gearset in _gameStateCache.Gearsets!) {
                if (gearset.ClassJob != this.Id) continue;
                
                TickScheduler.Schedule(delegate {
                    var command = $"/gs change {gearset.Slot}";
                    PluginLog.Debug($"Would send command: {command}");
                    XIVDeckPlugin.Instance.XivCommon.Functions.Chat.SendMessage(command);
                });

                return;
            }
            
            // pretty error handling
            var sheet = Injections.DataManager.Excel.GetSheet<ClassJob>();
            var classJob = sheet!.GetRow((uint) this.Id);

            if (classJob == null) 
                throw new ArgumentException($"Class with ID {this.Id} does not exist!");
            
            Injections.Chat.PrintError($"Couldn't switch to {classJob.NameEnglish.RawString} because you " +
                                       $"don't have a gearset for this class. Make one and try again.");
            session.SendText(JsonConvert.SerializeObject(new WSShowSDAlert(this.SDContext)));
        }

        public WSSwitchClassOpcode() : base("switchClass") { }
    }
}